namespace Poncho.Cli.Commands

module RemoveDoing =
    open FsToolkit.ErrorHandling
    open Poncho.Domain
    open Poncho.Local
    open Spectre.Console.Cli
    open System
    open System.ComponentModel
    open Poncho.Cli.Console.Format
    open Spectre.Console
    open Poncho.Cli.Journal.JournalComponents

    type Settings(name, dir) =
        inherit CommandSettings()

        [<Description("Doing Name")>]
        [<CommandArgument(0, "<name>")>]
        member val name: string = name

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
            |> Result.bind (fun journal -> Journal.removeDoing journal settings.name)
            |> Result.bind (fun journal -> LocalJournal.saveJournal journal dir)
            |> Result.bind (fun _ -> LocalJournal.loadJournal dir)
            |> Result.map (fun journal ->
                match Journal.lastEntry journal with
                | Some entry -> AnsiConsole.Write(EntryPreview entry) |> ignore
                | None -> ())
            |> Result.map (fun _ -> 0)
            |> Result.mapError (fun failure -> AnsiConsole.Markup(formatError failure))
            |> Result.defaultWith (fun _ -> 1)
