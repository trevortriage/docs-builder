// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using System.Collections.Frozen;
using Elastic.Markdown.IO.Configuration;
using Elastic.Markdown.IO.State;
using Microsoft.Extensions.Logging;

namespace Elastic.Markdown.CrossLinks;

public class ConfigurationCrossLinkFetcher(ConfigurationFile configuration, ILoggerFactory logger) : CrossLinkFetcher(logger)
{
	public override async Task<FetchedCrossLinks> Fetch()
	{
		var dictionary = new Dictionary<string, LinkReference>();
		var declaredRepositories = new HashSet<string>();
		foreach (var repository in configuration.CrossLinkRepositories)
		{
			_ = declaredRepositories.Add(repository);
			try
			{
				var linkReference = await Fetch(repository);
				dictionary.Add(repository, linkReference);
			}
			catch when (repository == "docs-content")
			{
				throw;
			}
			catch when (repository != "docs-content")
			{
				// TODO: ignored for now while we wait for all links.json files to populate
			}
		}

		return new FetchedCrossLinks
		{
			DeclaredRepositories = declaredRepositories,
			LinkReferences = dictionary.ToFrozenDictionary()
		};
	}


}
