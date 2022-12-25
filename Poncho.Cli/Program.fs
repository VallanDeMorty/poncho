namespace Poncho.Cli

module Startup =
    open Spectre.Console.Cli
    open Poncho.Cli.Commands

    let application =
        let cli = CommandApp()

        cli.Configure(fun config ->
            config
                .AddCommand<EmptyJournal.Handler>("journal")
                .WithDescription("creates an empty journal")
            |> ignore

            config
                .AddCommand<Today.Handler>("today")
                .WithDescription("initiates today entry to the journal")
            |> ignore

            config
                .AddCommand<AddDoing.Handler>("add")
                .WithDescription("adds a new doing to the journal")
            |> ignore

            config
                .AddCommand<RemoveDoing.Handler>("remove")
                .WithDescription("removes a doing from the journal")
            |> ignore

            config
                .AddCommand<SkipDoing.Handler>("skip")
                .WithDescription("skips a doing from the journal")
            |> ignore

            config
                .AddCommand<ReplaceDoing.Handler>("replace")
                .WithDescription("replaces a doing from the journal")
            |> ignore)

        cli

    [<EntryPoint>]
    let main argv = application.Run argv
