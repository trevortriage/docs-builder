// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using System.IO.Abstractions;
using Elastic.Markdown.Myst.Directives;
using Elastic.Markdown.Myst.FrontMatter;
using Markdig.Parsers;
using Markdig.Syntax;

namespace Elastic.Markdown.Myst.CodeBlocks;

public class AppliesToDirective(BlockParser parser, ParserContext context)
	: EnhancedCodeBlock(parser, context)
{
	public ApplicableTo? AppliesTo { get; set; }
}

public class EnhancedCodeBlock(BlockParser parser, ParserContext context)
	: FencedCodeBlock(parser), IBlockExtension
{
	public BuildContext Build { get; } = context.Build;

	public IFileInfo CurrentFile { get; } = context.Path;

	public bool SkipValidation { get; } = context.SkipValidation;

	public int OpeningLength => Info?.Length ?? 0 + 3;

	public List<CallOut> CallOuts { get; set; } = [];

	public IReadOnlyCollection<CallOut> UniqueCallOuts => [.. CallOuts.DistinctBy(c => c.Index)];

	public bool InlineAnnotations { get; set; }

	public string Language { get; set; } = "unknown";

	public string? Caption { get; set; }

	public string? ApiCallHeader { get; set; }
}
