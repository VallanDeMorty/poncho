namespace Poncho.Cli

module Startup =
    open Spectre.Console.Cli
    open Poncho.Cli.Commands

    let application =
        let cli = CommandApp()

        cli.Configure(fun config ->
            config
                .AddCommand<EmptyJournal.Handler>("empty-journal")
                .WithDescription("creates an empty journal")
            |> ignore

            config
                .AddCommand<Today.Handler>("today")
                .WithDescription("initiates today entry to the journal")
            |> ignore

            config
                .AddCommand<AddDoing.Handler>("add-doing")
                .WithDescription("adds a new doing to the journal")
            |> ignore

            config
                .AddCommand<RemoveDoing.Handler>("remove-doing")
                .WithDescription("removes a doing from the journal")
            |> ignore

            config
                .AddCommand<SkipDoing.Handler>("skip-doing")
                .WithDescription("skips a doing from the journal")
            |> ignore

            config
                .AddCommand<ReplaceDoing.Handler>("replace-doing")
                .WithDescription("replaces a doing from the journal")
            |> ignore)

        cli

    [<EntryPoint>]
    let main argv = application.Run argv
