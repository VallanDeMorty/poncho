namespace Poncho.Cli.Commands

module ViewJournal =
    open FsToolkit.ErrorHandling
    open Poncho.Cli.Console.Format
    open Poncho.Cli.DateTime
    open Poncho.Cli.Journal.JournalComponents
    open Poncho.Domain
    open Poncho.Local
    open Spectre.Console
    open Spectre.Console.Cli
    open System
    open System.ComponentModel

    type Settings(tillDate, dir) =
        inherit CommandSettings()

        [<Description("Time Interval to Look")>]
        [<CommandOption("-i|--interval")>]
        member val tillDate: string =
            match tillDate with
            | null -> "7 days ago"
            | _ -> tillDate

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

            let tillDate = NaturalDateTime.parse settings.tillDate

            LocalJournal.loadJournal dir
            |> Result.zip tillDate
            |> Result.bind (fun (tillDate, journal) -> Journal.listEntriesTillDate journal tillDate)
            |> Result.zip tillDate
            |> Result.map (fun (tillDate, entries) ->
                AnsiConsole.MarkupLine(JournalTitle DateTime.UtcNow tillDate)

                entries
                |> List.iter (fun entry ->
                    AnsiConsole.MarkupLine("")
                    AnsiConsole.Write(EntryPreview entry))

                0)
            |> Result.mapError (fun failure -> AnsiConsole.Markup(formatError failure))
            |> Result.defaultWith (fun _ -> 1)
