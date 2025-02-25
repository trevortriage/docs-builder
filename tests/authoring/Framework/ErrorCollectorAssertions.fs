// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

namespace authoring

open System.Diagnostics
open System.Linq
open Elastic.Markdown.Diagnostics
open FsUnitTyped
open Swensen.Unquote

[<AutoOpen>]
module DiagnosticsCollectorAssertions =

    [<DebuggerStepThrough>]
    let hasNoErrors (actual: Lazy<GeneratorResults>) =
        let actual = actual.Value
        let errors = actual.Context.Collector.Errors
        test <@ errors = 0 @>

    [<DebuggerStepThrough>]
    let hasError (expected: string) (actual: Lazy<GeneratorResults>) =
        let actual = actual.Value
        actual.Context.Collector.Errors |> shouldBeGreaterThan 0
        let errorDiagnostics = actual.Context.Collector.Diagnostics
                                   .Where(fun d -> d.Severity = Severity.Error)
                                   .ToArray()
                                   |> List.ofArray
                                   |> List.tryHead

        match errorDiagnostics with
        | Some e ->
            let message = e.Message
            test <@ message.Contains(expected) @>
        | None -> failwithf "Expected errors but no errors were logged"

        
    let hasNoWarnings (actual: Lazy<GeneratorResults>) =
        let actual = actual.Value
        let warnings = actual.Context.Collector.Warnings
        test <@ warnings = 0 @>

    [<DebuggerStepThrough>]
    let hasWarning (expected: string) (actual: Lazy<GeneratorResults>) =
        let actual = actual.Value
        actual.Context.Collector.Warnings |> shouldBeGreaterThan 0
        let errorDiagnostics = actual.Context.Collector.Diagnostics
                                   .Where(fun d -> d.Severity = Severity.Warning)
                                   .ToArray()
                                   |> List.ofArray
                                   |> List.tryHead
        match errorDiagnostics with
        | Some e ->
            let message = e.Message
            test <@ message.Contains(expected) @>
        | None -> failwithf "Expected errors but no errors were logged"
