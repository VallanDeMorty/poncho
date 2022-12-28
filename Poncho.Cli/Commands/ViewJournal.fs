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

    type Settings(dir) =
        inherit CommandSettings()

        [<Description("Time Interval to Look")>]
        [<CommandOption("-i|--interval")>]
        member val tillDate: string = "7 days ago"

        [<Description("Directory to Look for the Journal")>]
        [<CommandOption("-d|--dir")>]
        member val dir: string Option = dir

    type Handler() =
        inherit Command<Settings>()

        override _.Execute(_, settings) =
            let dir =
                settings.dir
                |> Option.bind (fun providedDir -> if providedDir.Length = 0 then None else Some(providedDir))
                |> Option.defaultWith (fun () -> Environment.CurrentDirectory)

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
