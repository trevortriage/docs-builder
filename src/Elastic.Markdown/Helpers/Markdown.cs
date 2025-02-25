// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

namespace Elastic.Markdown.Helpers;

public static class MarkdownStringExtensions
{
	public static string StripMarkdown(this string markdown)
	{
		using var writer = new StringWriter();
		_ = Markdig.Markdown.ToPlainText(markdown, writer);
		return writer.ToString().TrimEnd('\n');
	}
}
