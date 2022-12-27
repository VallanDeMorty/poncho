namespace Poncho.Cli.Commands

module AddDoing =
    open FsToolkit.ErrorHandling
    open Microsoft.Recognizers.Text
    open Microsoft.Recognizers.Text.DateTime
    open Poncho.Cli.Console.Format
    open Poncho.Domain
    open Poncho.Local
    open Spectre.Console
    open Spectre.Console.Cli
    open System
    open System.Collections.Generic
    open System.ComponentModel

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

        [<Description("Directory")>]
        [<CommandOption("-d|--dir")>]
        member val dir: string Option = dir

        [<Description("Last Date When You Did It")>]
        [<CommandOption("-l|--last-date")>]
        member val lastDate: string = lastDate

    type Handler() =
        inherit Command<Settings>()

        let parseDateTime dateTime =
            try
                DateTimeRecognizer.RecognizeDateTime(dateTime, Culture.English)
                |> Ok
                |> Result.bind (fun models ->
                    match models.Count with
                    | 0 -> Error $"No dates found in the '{dateTime}."
                    | _ -> models.Item(0) |> Ok)
                |> Result.bind (fun model ->
                    match (model.TypeName.Contains "datetime", model.TypeName.Contains "range") with
                    | (true, false) -> model |> Ok
                    | _ -> Error $"No dates found in the '{dateTime}.")
                |> Result.bind (fun model ->
                    match model.Resolution["values"] with
                    | :? IList<Dictionary<string, string>> as values -> values |> Ok
                    | _ -> Error $"No dates found in the '{dateTime}.")
                |> Result.bind (fun values ->
                    match values.Count with
                    | 0 -> Error $"No dates found in the '{dateTime}."
                    | _ -> values.Item(0) |> Ok)
                |> Result.bind (fun value ->
                    match value.TryGetValue "value" with
                    | true, rawDateTime -> (DateTime.TryParse(rawDateTime), rawDateTime) |> Ok
                    | _ -> Error $"No dates found in the '{dateTime}.")
                |> Result.bind (fun (parsed, rawDateTime) ->
                    match parsed with
                    | (true, dateTime) -> dateTime |> Ok
                    | _ ->
                        Error $"Failed to parse the natural date from '{rawDateTime}' and in partical '{rawDateTime}'.")
            with :? Exception ->
                $"Failed to parse the natural date from '{dateTime}'." |> Error

        override _.Execute(_, settings) =
            let dir =
                settings.dir
                |> Option.bind (fun providedDir -> if providedDir.Length = 0 then None else Some(providedDir))
                |> Option.defaultWith (fun () -> Environment.CurrentDirectory)

            let lastDate =
                match settings.lastDate.Length with
                | 0 -> None
                | _ -> parseDateTime settings.lastDate |> Some

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
            |> Result.map (fun _ -> 0)
            |> Result.mapError (fun failure -> AnsiConsole.Markup(formatError failure))
            |> Result.defaultWith (fun _ -> 1)
