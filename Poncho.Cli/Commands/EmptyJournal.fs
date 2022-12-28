namespace Poncho.Cli.Commands

module EmptyJournal =
    open FsToolkit.ErrorHandling
    open Poncho.Domain
    open Poncho.Local
    open Spectre.Console.Cli
    open System
    open System.ComponentModel
    open Poncho.Cli.Console.Format
    open Spectre.Console

    type Settings(force, dir) =
        inherit CommandSettings()

        [<Description("Force to Rewrite")>]
        [<CommandOption("-f|--force")>]
        member val force: bool = force

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

            let journal = Journal.emptyJournal |> Journal.initialize <| { doingsPerDay = 3 }

            let saveJournal =
                match settings.force with
                | true -> LocalJournal.saveJournal
                | false -> LocalJournal.saveIfNotExists

            saveJournal journal dir
            |> Result.map (fun _ -> 0)
            |> Result.mapError (fun failure -> AnsiConsole.Markup(formatError failure))
            |> Result.defaultWith (fun _ -> 1)
