// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

namespace authoring

open System
open System.Collections.Generic
open System.Collections.Frozen
open System.Runtime.InteropServices
open System.Threading.Tasks
open Elastic.Markdown.CrossLinks
open Elastic.Markdown.IO.Configuration
open Elastic.Markdown.IO.State

type TestCrossLinkResolver (config: ConfigurationFile) =

    let references = Dictionary<string, LinkReference>()
    let declared = HashSet<string>()

    member this.LinkReferences = references
    member this.DeclaredRepositories = declared

    interface ICrossLinkResolver with
        member this.FetchLinks() =
            let redirects = LinkReference.SerializeRedirects config.Redirects
            // language=json
            let json = $$"""{
  "origin": {
    "branch": "main",
    "remote": " https://github.com/elastic/docs-content",
    "ref": "76aac68d066e2af935c38bca8ce04d3ee67a8dd9"
  },
  "url_path_prefix": "/elastic/docs-content/tree/main",
  "cross_links": [],
  "redirects" : {{redirects}},
  "links": {
    "index.md": {},
    "get-started/index.md": {
      "anchors": [
        "elasticsearch-intro-elastic-stack",
        "elasticsearch-intro-use-cases"
      ]
    },
    "solutions/observability/apps/apm-server-binary.md": {
      "anchors": [ "apm-deb" ]
    },
    "testing/redirects/first-page.md": {
      "anchors": [ "current-anchor", "another-anchor" ]
    },
    "testing/redirects/second-page.md": {
      "anchors": [ "active-anchor", "zz" ]
    },
    "testing/redirects/third-page.md": { "anchors": [ "bb" ] },
    "testing/redirects/5th-page.md": { "anchors": [ "yy" ] }
  }
}
"""
            let reference = CrossLinkFetcher.Deserialize json
            this.LinkReferences.Add("docs-content", reference)
            this.LinkReferences.Add("kibana", reference)
            this.DeclaredRepositories.Add("docs-content") |> ignore;
            this.DeclaredRepositories.Add("kibana") |> ignore;
            this.DeclaredRepositories.Add("elasticsearch") |> ignore;
            let crossLinks =
                FetchedCrossLinks(
                    DeclaredRepositories=this.DeclaredRepositories,
                    LinkReferences=this.LinkReferences.ToFrozenDictionary()
                )
            Task.FromResult crossLinks

        member this.TryResolve(errorEmitter, crossLinkUri, [<Out>]resolvedUri : byref<Uri|null>) =
            let crossLinks =
                FetchedCrossLinks(
                    DeclaredRepositories=this.DeclaredRepositories,
                    LinkReferences=this.LinkReferences.ToFrozenDictionary()
                )
            CrossLinkResolver.TryResolve(errorEmitter, crossLinks, crossLinkUri, &resolvedUri);


