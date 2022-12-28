namespace Poncho.Cli.Journal

module JournalComponents =
    open Spectre.Console
    open Poncho.Domain.Journal
    open System

    let friendlyDateFormat = "MMMM dd, yyyy"

    let JournalTitle (fromDate: DateTime) (tillDate: DateTime) =
        $"[bold]Journal[/] from [bold]{fromDate.ToString(friendlyDateFormat)}[/] to [bold]{tillDate.ToString(friendlyDateFormat)}[/]"

    let EntryPreview (entry: JournalEntry) =
        let entryDate = new Tree($"[bold]{entry.date.ToString(friendlyDateFormat)}[/]")

        let doings = entryDate.AddNode("Doings")

        entry.doings
        |> List.map (fun doing -> (doings.AddNode($"[bold]{doing.title}[/] ({doing.name})"), doing))
        |> List.iter (fun (doingNode, doing) ->
            doingNode.AddNode($"Threshold: [bold]{doing.threshold}[/]d") |> ignore
            doingNode.AddNode($"Current: [bold]{doing.current}[/]d") |> ignore)

        match entry.commitments with
        | [] -> ()
        | _ ->
            let commitments = entryDate.AddNode("Commitments")

            entry.commitments
            |> List.map (fun commitment -> entry.doings |> List.tryFind (fun doing -> doing.name = commitment))
            |> List.filter Option.isSome
            |> List.map Option.get
            |> List.map (fun commitment -> commitments.AddNode($"[bold]{commitment.title}[/]"))
            |> ignore

        match entry.newDoings with
        | [] -> ()
        | _ ->
            let newDoings = entryDate.AddNode("New Doings")

            entry.newDoings
            |> List.map (fun newDoing -> entry.doings |> List.tryFind (fun doing -> doing.name = newDoing))
            |> List.filter Option.isSome
            |> List.map Option.get
            |> List.map (fun newDoing -> newDoings.AddNode($"[bold]{newDoing.title}[/]"))
            |> ignore

        match entry.removedDoings with
        | [] -> ()
        | _ ->
            let removedDoings = entryDate.AddNode("Removed Doings")

            entry.removedDoings
            |> List.map (fun removedDoing -> entry.doings |> List.tryFind (fun doing -> doing.name = removedDoing))
            |> List.filter Option.isSome
            |> List.map Option.get
            |> List.map (fun removedDoing -> removedDoings.AddNode($"[bold]{removedDoing.title}[/]"))
            |> ignore

        entryDate
