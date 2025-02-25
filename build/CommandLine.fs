// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

module CommandLine

open Argu
open Microsoft.FSharp.Reflection
open System
open Bullseye

type Build =
    | [<CliPrefix(CliPrefix.None);SubCommand>] Clean
    | [<CliPrefix(CliPrefix.None);SubCommand>] Version
    | [<CliPrefix(CliPrefix.None);Hidden;SubCommand>] Compile
    | [<CliPrefix(CliPrefix.None);SubCommand>] Build
    | [<CliPrefix(CliPrefix.None);SubCommand>] Test
    
    | [<CliPrefix(CliPrefix.None);SubCommand>] Format
    | [<CliPrefix(CliPrefix.None);SubCommand>] Watch

    | [<CliPrefix(CliPrefix.None);Hidden;SubCommand>] Lint
    | [<CliPrefix(CliPrefix.None);Hidden;SubCommand>] PristineCheck
    | [<CliPrefix(CliPrefix.None);Hidden;SubCommand>] ValidateLicenses

    | [<CliPrefix(CliPrefix.None);SubCommand>] Publish
    | [<CliPrefix(CliPrefix.None);Hidden;SubCommand>] PublishBinaries
    | [<CliPrefix(CliPrefix.None);Hidden;SubCommand>] PublishContainers
    | [<CliPrefix(CliPrefix.None);Hidden;SubCommand>] PublishZip

    | [<CliPrefix(CliPrefix.None);SubCommand>] ReleaseNotes
    | [<CliPrefix(CliPrefix.None);SubCommand>] Release
    
    | [<Inherit;AltCommandLine("-s")>] Single_Target
    | [<Inherit>] Token of string 
    | [<Inherit;AltCommandLine("-c")>] Skip_Dirty_Check
with
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            // commands
            | Clean -> "clean known output locations"
            | Version -> "print version information"
            | Build -> "Run build"
            
            | Test -> "runs a clean build and then runs all the tests "
            | Release -> "runs build, tests, and create and validates the packages shy of publishing them"
            | Publish -> "Publishes artifacts"
            | Format -> "runs dotnet format"

            | Watch -> "runs dotnet watch to continuous build code/templates and web assets on the fly"

            // steps
            | Lint
            | PristineCheck
            | PublishBinaries
            | PublishContainers
            | PublishZip
            | ValidateLicenses
            | ReleaseNotes
            | Compile 

            // flags
            | Single_Target -> "Runs the provided sub command without running their dependencies"
            | Token _ -> "Token to be used to authenticate with github"
            | Skip_Dirty_Check -> "Skip the clean checkout check that guards the release/publish targets"

    member this.StepName =
        match FSharpValue.GetUnionFields(this, typeof<Build>) with
        | case, _ -> case.Name.ToLowerInvariant()
        
    static member Targets =
        let cases = FSharpType.GetUnionCases(typeof<Build>)
        seq {
             for c in cases do
                 if c.GetFields().Length = 0 then
                     match FSharpValue.MakeUnion(c, [| |]) with
                     | NonNull u -> u :?> Build
                     | _ -> failwithf $"%s{c.Name} can not be cast to Build enum"
        }
        
    static member Ignore (_: Build) _ = ()
        
    static member Step action (target: Build) parsed =
        Targets.Target(target.StepName, Action(fun _ -> action(parsed)))

    static member Cmd (dependsOn: Build list) (composedOf: Build list) action (target: Build) (parsed: ParseResults<Build>) =
        let singleTarget = parsed.TryGetResult Single_Target |> Option.isSome
        let dependsOn = if singleTarget then [] else dependsOn
            
        let steps = dependsOn @ composedOf |> List.map _.StepName
        Targets.Target(target.StepName, steps, Action(fun _ -> action parsed))