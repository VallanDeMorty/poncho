namespace Poncho.Cli.Commands

module Today =
    open FsToolkit.ErrorHandling
    open Poncho.Cli.Console.Format
    open Poncho.Cli.Journal.JournalComponents
    open Poncho.Domain
    open Poncho.Local
    open Spectre.Console
    open Spectre.Console.Cli
    open System
    open System.ComponentModel

    type Settings(dir) =
        inherit CommandSettings()

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
            |> Result.map Journal.today
            |> Result.bind (fun journal -> LocalJournal.saveJournal journal dir)
            |> Result.bind (fun _ -> LocalJournal.loadJournal dir)
            |> Result.map (fun journal ->
                match Journal.lastEntry journal with
                | Some entry -> AnsiConsole.Write(EntryPreview entry) |> ignore
                | None -> ())
            |> Result.map (fun _ -> 0)
            |> Result.mapError (fun failure -> AnsiConsole.Markup(formatError failure))
            |> Result.defaultWith (fun _ -> 1)
