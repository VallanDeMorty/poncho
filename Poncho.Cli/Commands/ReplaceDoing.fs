namespace Poncho.Cli.Commands

module ReplaceDoing =
    open FsToolkit.ErrorHandling
    open Poncho.Domain
    open Poncho.Local
    open Spectre.Console.Cli
    open System
    open System.ComponentModel
    open Poncho.Cli.Console.Format
    open Spectre.Console

    type Settings(originalName, newName, dir) =
        inherit CommandSettings()

        [<Description("Original Doing Name")>]
        [<CommandArgument(0, "<originalName>")>]
        member val originalName: string = originalName

        [<Description("New Doing Name")>]
        [<CommandArgument(0, "<newName>")>]
        member val newName: string = newName

        [<Description("Directory to Look for the Journal")>]
        [<CommandOption("-d|--dir")>]
        member val dir: string =
            match dir with
            | null -> ""
            | _ -> dir

    type Handler() =
        inherit Command<Settings>()

        override _.Execute(_, settings) =
            let dir =
                match settings.dir with
                | dirPath when dirPath.Length > 0 -> dirPath
                | _ -> Environment.CurrentDirectory

            LocalJournal.loadJournal dir
            |> Result.bind (fun journal -> Journal.replace journal settings.originalName settings.newName)
            |> Result.bind (fun journal -> LocalJournal.saveJournal journal dir)
            |> Result.map (fun _ -> 0)
            |> Result.mapError (fun failure -> AnsiConsole.Markup(formatError failure))
            |> Result.defaultWith (fun _ -> 1)
