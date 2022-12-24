namespace Poncho.Cli.Commands

module AddDoing =
    open FsToolkit.ErrorHandling
    open Poncho.Domain
    open Poncho.Local
    open Spectre.Console.Cli
    open System
    open System.ComponentModel
    open Poncho.Cli.Console.Format
    open Spectre.Console

    type Settings(name, title, threshold, dir) =
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

            let newDoing =
                Journal.verifyDoing (
                    { name = settings.name
                      title = settings.title
                      threshold = settings.threshold
                      current = 0 }
                )

            LocalJournal.loadJournal dir |> Result.map2 Journal.addDoing <| newDoing
            |> Result.bind (fun journal -> LocalJournal.saveJournal journal dir)
            |> Result.map (fun _ -> 0)
            |> Result.mapError (fun failure -> AnsiConsole.Markup(formatError failure))
            |> Result.defaultWith (fun _ -> 1)
