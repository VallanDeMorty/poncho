namespace Poncho.Cli

module Startup =
    open Spectre.Console.Cli
    open Poncho.Cli.Commands

    let application =
        let cli = CommandApp()

        cli.Configure(fun config ->
            config
                .AddCommand<EmptyJournal.Handler>("journal")
                .WithDescription("Create an Empty Journal")
            |> ignore

            config
                .AddCommand<ViewJournal.Handler>("view")
                .WithDescription("View the Journal")
            |> ignore

            config
                .AddCommand<Today.Handler>("today")
                .WithDescription("Initiate Today Entry to the Journal")
            |> ignore

            config
                .AddCommand<AddDoing.Handler>("add")
                .WithDescription("Add a New Doing to the Journal")
            |> ignore

            config
                .AddCommand<RemoveDoing.Handler>("remove")
                .WithDescription("Remove a Doing from the Journal")
            |> ignore

            config
                .AddCommand<CommitDoing.Handler>("commit")
                .WithDescription("Commit a Doing Today in the Journal")
            |> ignore

            config
                .AddCommand<SkipDoing.Handler>("skip")
                .WithDescription("Skip a Doing from Today in the Journal")
            |> ignore

            config
                .AddCommand<ReplaceDoing.Handler>("replace")
                .WithDescription("Replace a Commited Today Doing in the Journal")
            |> ignore)

        cli

    [<EntryPoint>]
    let main argv = application.Run argv
