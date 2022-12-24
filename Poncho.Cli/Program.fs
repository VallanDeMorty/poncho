﻿namespace Poncho.Cli

module Startup =
    open Spectre.Console.Cli
    open Poncho.Cli.Commands

    let application =
        let cli = CommandApp()

        cli.Configure(fun config ->
            config
                .AddCommand<EmptyJournal.Handler>("empty-journal")
                .WithDescription("creates an empty journal")
            |> ignore)

        cli

    [<EntryPoint>]
    let main argv = application.Run argv