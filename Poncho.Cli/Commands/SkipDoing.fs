namespace Poncho.Cli.Commands

module SkipDoing =
    open FsToolkit.ErrorHandling
    open Poncho.Domain
    open Poncho.Local
    open Spectre.Console.Cli
    open System
    open System.ComponentModel
    open Poncho.Cli.Console.Format
    open Spectre.Console

    type Settings(name, dir) =
        inherit CommandSettings()

        [<Description("Doing Name")>]
        [<CommandArgument(0, "<name>")>]
        member val name: string = name

        [<Description("directory")>]
        [<CommandOption("-d|--dir")>]
        member val dir: string Option = dir

    type Handler() =
        inherit Command<Settings>()

        override _.Execute(_, settings) =
            let dir =
                settings.dir
                |> Option.bind (fun providedDir -> if providedDir.Length = 0 then None else Some(providedDir))
                |> Option.defaultWith (fun () -> Environment.CurrentDirectory)

            LocalJournal.loadJournal dir
            |> Result.map (fun journal -> Journal.skip journal settings.name)
            |> Result.bind (fun journal -> LocalJournal.saveJournal journal dir)
            |> Result.map (fun _ -> 0)
            |> Result.mapError (fun failure -> AnsiConsole.Markup(formatError failure))
            |> Result.defaultWith (fun _ -> 1)
