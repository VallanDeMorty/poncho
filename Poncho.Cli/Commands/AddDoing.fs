namespace Poncho.Cli.Commands

module AddDoing =
    open FsToolkit.ErrorHandling
    open Poncho.Cli.Console.Format
    open Poncho.Cli.DateTime
    open Poncho.Domain
    open Poncho.Local
    open Spectre.Console
    open Spectre.Console.Cli
    open System
    open System.ComponentModel
    open Poncho.Cli.Journal.JournalComponents

    type Settings(name, title, threshold, lastDate, dir) =
        inherit CommandSettings()

        [<Description("Doing Name")>]
        [<CommandArgument(0, "<name>")>]
        member val name: string = name

        [<Description("Doing Title")>]
        [<CommandArgument(1, "<title>")>]
        member val title: string = title

        [<Description("Doing Threshold")>]
        [<CommandArgument(2, "<threshold>")>]
        member val threshold: int = threshold

        [<Description("Directory to Look for the Journal")>]
        [<CommandOption("-d|--dir")>]
        member val dir: string =
            match dir with
            | null -> ""
            | _ -> dir

        [<Description("Last Date When You Did It")>]
        [<CommandOption("-l|--last-date")>]
        member val lastDate: string =
            match lastDate with
            | null -> ""
            | _ -> lastDate

    type Handler() =
        inherit Command<Settings>()

        override _.Execute(_, settings) =
            let dir =
                match settings.dir with
                | dirPath when dirPath.Length > 0 -> dirPath
                | _ -> Environment.CurrentDirectory

            let lastDate =
                match settings.lastDate.Length with
                | 0 -> None
                | _ -> NaturalDateTime.parse settings.lastDate |> Some

            let addDoing journal doing =
                match lastDate with
                | Some(Ok lastDate) -> Journal.addDoingPerformedLastTime journal doing lastDate
                | Some(Error error) -> Error error
                | None -> Journal.addUnknownDoing journal doing

            let newDoing =
                Journal.verifyDoing (
                    { name = settings.name
                      title = settings.title
                      threshold = settings.threshold
                      current = 0 }
                )

            LocalJournal.loadJournal dir |> Result.zip <| newDoing
            |> Result.bind (fun (journal, doing) -> addDoing journal doing)
            |> Result.bind (fun journal -> LocalJournal.saveJournal journal dir)
            |> Result.bind (fun _ -> LocalJournal.loadJournal dir)
            |> Result.map (fun journal ->
                match Journal.lastEntry journal with
                | Some entry -> AnsiConsole.Write(EntryPreview entry) |> ignore
                | None -> ())
            |> Result.map (fun _ -> 0)
            |> Result.mapError (fun failure -> AnsiConsole.Markup(formatError failure))
            |> Result.defaultWith (fun _ -> 1)
