// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using System.Diagnostics.CodeAnalysis;
using Elastic.Markdown.Diagnostics;
using Elastic.Markdown.Slices.Directives;
using Markdig.Helpers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using RazorSlices;

namespace Elastic.Markdown.Myst.CodeBlocks;

public class EnhancedCodeBlockHtmlRenderer : HtmlObjectRenderer<EnhancedCodeBlock>
{
	private const int TabWidth = 4;

	[SuppressMessage("Reliability", "CA2012:Use ValueTasks correctly")]
	private static void RenderRazorSlice<T>(RazorSlice<T> slice, HtmlRenderer renderer, EnhancedCodeBlock block)
	{
		var html = slice.RenderAsync().GetAwaiter().GetResult();
		var blocks = html.Split("[CONTENT]", 2, StringSplitOptions.RemoveEmptyEntries);
		_ = renderer.Write(blocks[0]);
		RenderCodeBlockLines(renderer, block);
		_ = renderer.Write(blocks[1]);
	}

	/// <summary>
	/// Renders the code block lines while also removing the common indentation level.
	/// Required because EnableTrackTrivia preserves extra indentation.
	/// </summary>
	private static void RenderCodeBlockLines(HtmlRenderer renderer, EnhancedCodeBlock block)
	{
		var commonIndent = GetCommonIndent(block);
		var hasCode = false;
		for (var i = 0; i < block.Lines.Count; i++)
		{
			var line = block.Lines.Lines[i];
			var slice = line.Slice;
			//ensure we never emit an empty line at beginning or start
			if ((i == 0 || i == block.Lines.Count - 1) && line.Slice.IsEmptyOrWhitespace())
				continue;
			var indent = CountIndentation(slice);
			if (indent >= commonIndent)
				slice.Start += commonIndent;

			if (!hasCode)
			{
				_ = renderer.Write($"<code class=\"language-{block.Language}\">");
				hasCode = true;
			}
			RenderCodeBlockLine(renderer, block, slice, i);
		}
		if (hasCode)
			_ = renderer.Write($"</code>");
	}

	private static void RenderCodeBlockLine(HtmlRenderer renderer, EnhancedCodeBlock block, StringSlice slice, int lineNumber)
	{
		_ = renderer.WriteEscape(slice);
		RenderCallouts(renderer, block, lineNumber);
		_ = renderer.WriteLine();
	}

	private static void RenderCallouts(HtmlRenderer renderer, EnhancedCodeBlock block, int lineNumber)
	{
		var callOuts = FindCallouts(block.CallOuts, lineNumber + 1);
		foreach (var callOut in callOuts)
			_ = renderer.Write($"<span class=\"code-callout\" data-index=\"{callOut.Index}\">{callOut.Index}</span>");
	}

	private static IEnumerable<CallOut> FindCallouts(
		IEnumerable<CallOut> callOuts,
		int lineNumber
	) => callOuts.Where(callOut => callOut.Line == lineNumber);

	private static int GetCommonIndent(EnhancedCodeBlock block)
	{
		var commonIndent = int.MaxValue;
		for (var i = 0; i < block.Lines.Count; i++)
		{
			var line = block.Lines.Lines[i].Slice;
			if (line.IsEmptyOrWhitespace())
				continue;
			var indent = CountIndentation(line);
			commonIndent = Math.Min(commonIndent, indent);
		}
		return commonIndent;
	}


	private static int CountIndentation(StringSlice slice)
	{
		var indentCount = 0;
		for (var i = slice.Start; i <= slice.End; i++)
		{
			var c = slice.Text[i];
			if (c == ' ')
				indentCount++;
			else if (c == '\t')
				indentCount += TabWidth;
			else
				break;
		}
		return indentCount;
	}

	protected override void Write(HtmlRenderer renderer, EnhancedCodeBlock block)
	{
		if (block is AppliesToDirective appliesToDirective)
		{
			RenderAppliesToHtml(renderer, appliesToDirective);
			return;
		}

		var callOuts = block.UniqueCallOuts;

		var slice = Code.Create(new CodeViewModel
		{
			CrossReferenceName = string.Empty,// block.CrossReferenceName,
			Language = block.Language,
			Caption = block.Caption,
			ApiCallHeader = block.ApiCallHeader
		});

		RenderRazorSlice(slice, renderer, block);

		if (!block.InlineAnnotations && callOuts.Count > 0)
		{
			var index = block.Parent!.IndexOf(block);
			if (index == block.Parent!.Count - 1)
				block.EmitError("Code block with annotations is not followed by any content, needs numbered list");
			else
			{
				var siblingBlock = block.Parent[index + 1];
				if (siblingBlock is not ListBlock)
				{
					//allow one block of content in between
					if (index + 2 <= (block.Parent!.Count - 1))
						siblingBlock = block.Parent[index + 2];
					if (siblingBlock is not ListBlock)
						block.EmitError("Code block with annotations is not followed by a list");
				}
				if (siblingBlock is ListBlock l && l.Count < callOuts.Count)
				{
					block.EmitError(
						$"Code block has {callOuts.Count} callouts but the following list only has {l.Count}");
				}
				else if (siblingBlock is ListBlock listBlock)
				{
					_ = block.Parent.Remove(listBlock);
					_ = renderer.WriteLine("<ol class=\"code-callouts\">");
					foreach (var child in listBlock)
					{
						var listItem = (ListItemBlock)child;
						var previousImplicit = renderer.ImplicitParagraph;
						renderer.ImplicitParagraph = !listBlock.IsLoose;

						_ = renderer.EnsureLine();
						if (renderer.EnableHtmlForBlock)
						{
							_ = renderer.Write("<li");
							_ = renderer.WriteAttributes(listItem);
							_ = renderer.Write('>');
						}

						renderer.WriteChildren(listItem);

						if (renderer.EnableHtmlForBlock)
							_ = renderer.WriteLine("</li>");

						_ = renderer.EnsureLine();
						renderer.ImplicitParagraph = previousImplicit;
					}
					_ = renderer.WriteLine("</ol>");
				}
			}
		}
		else if (block.InlineAnnotations)
		{
			_ = renderer.WriteLine("<ol class=\"code-callouts\">");
			foreach (var c in block.UniqueCallOuts)
			{
				_ = renderer.WriteLine("<li>");
				_ = renderer.WriteLine(c.Text);
				_ = renderer.WriteLine("</li>");
			}

			_ = renderer.WriteLine("</ol>");
		}
	}

	[SuppressMessage("Reliability", "CA2012:Use ValueTasks correctly")]
	private static void RenderAppliesToHtml(HtmlRenderer renderer, AppliesToDirective appliesToDirective)
	{
		var appliesTo = appliesToDirective.AppliesTo;
		var slice2 = ApplicableTo.Create(appliesTo);
		if (appliesTo is null || appliesTo == FrontMatter.ApplicableTo.All)
			return;
		var html = slice2.RenderAsync().GetAwaiter().GetResult();
		_ = renderer.Write(html);
	}
}
