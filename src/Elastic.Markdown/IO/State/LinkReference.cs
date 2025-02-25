// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using System.Text.Json;
using System.Text.Json.Serialization;
using Elastic.Markdown.IO.Discovery;

namespace Elastic.Markdown.IO.State;

public record LinkMetadata
{
	[JsonPropertyName("anchors")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string[]? Anchors { get; init; } = [];

	[JsonPropertyName("hidden")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public bool Hidden { get; init; }
}

public record LinkSingleRedirect
{
	[JsonPropertyName("anchors")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public IReadOnlyDictionary<string, string?>? Anchors { get; init; }

	[JsonPropertyName("to")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string? To { get; init; }
}

public record LinkRedirect : LinkSingleRedirect
{
	public static IReadOnlyDictionary<string, string?> CatchAllAnchors { get; } = new Dictionary<string, string?> { { "!", "!" } };

	[JsonPropertyName("many")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public LinkSingleRedirect[]? Many { get; init; }
}

public record LinkReference
{
	[JsonPropertyName("origin")]
	public required GitCheckoutInformation Origin { get; init; }

	[JsonPropertyName("url_path_prefix")]
	public required string? UrlPathPrefix { get; init; }

	/// Mapping of relative filepath and all the page's anchors for deep links
	[JsonPropertyName("links")]
	public required Dictionary<string, LinkMetadata> Links { get; init; } = [];

	[JsonPropertyName("cross_links")]
	public required string[] CrossLinks { get; init; } = [];

	/// Mapping of relative filepath and all the page's anchors for deep links
	[JsonPropertyName("redirects")]
	public Dictionary<string, LinkRedirect>? Redirects { get; init; }

	public static string SerializeRedirects(Dictionary<string, LinkRedirect>? redirects) =>
		JsonSerializer.Serialize(redirects, SourceGenerationContext.Default.DictionaryStringLinkRedirect);

	public static LinkReference Deserialize(string json) =>
		JsonSerializer.Deserialize(json, SourceGenerationContext.Default.LinkReference)!;

	public static LinkReference Create(DocumentationSet set)
	{
		var redirects = set.Configuration.Redirects;
		var crossLinks = set.Context.Collector.CrossLinks.ToHashSet().ToArray();
		var links = set.MarkdownFiles.Values
			.Select(m => (m.RelativePath, File: m))
			.ToDictionary(k => k.RelativePath, v =>
			{
				var anchors = v.File.Anchors.Count == 0 ? null : v.File.Anchors.ToArray();
				return new LinkMetadata { Anchors = anchors, Hidden = v.File.Hidden };
			});

		return new LinkReference
		{
			Redirects = redirects,
			UrlPathPrefix = set.Context.UrlPathPrefix,
			Origin = set.Context.Git,
			Links = links,
			CrossLinks = crossLinks
		};
	}
}
