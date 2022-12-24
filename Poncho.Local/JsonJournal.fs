namespace Poncho.Local

module JsonJournal =
    open System
    open Poncho.Domain.Journal
    open FSharp.Json

    type JsonJournalEntry =
        { date: DateTime
          doings: Doing list
          plan: DoingName list
          commitments: DoingName list
          newDoings: DoingName list
          removedDoings: DoingName list }

    type JsonJournal =
        { history: JsonJournalEntry list
          metrics: Metrics }

    let toJsonJournal (journal: Journal) =
        { history =
            journal.history
            |> List.map (fun entry ->
                { date = entry.date.ToDateTime(TimeOnly.MinValue)
                  doings = entry.doings
                  plan = entry.plan
                  commitments = entry.commitments
                  newDoings = entry.newDoings
                  removedDoings = entry.removedDoings })
          metrics = journal.metrics }

    let fromJsonJournal (jsonJournal: JsonJournal) : Journal =
        { history =
            jsonJournal.history
            |> List.map (fun entry ->
                { date = DateOnly.FromDateTime entry.date
                  doings = entry.doings
                  plan = entry.plan
                  commitments = entry.commitments
                  newDoings = entry.newDoings
                  removedDoings = entry.removedDoings })
          metrics = jsonJournal.metrics }

    let serializeJournal (journal: Journal) =
        try
            toJsonJournal journal |> Json.serialize |> Ok
        with :? Exception as ex ->
            let message = sprintf "Failed to serialize journal: %s" ex.Message
            Error message

    let deserializeJournal json =
        try
            Json.deserialize<JsonJournal> json |> fromJsonJournal |> Ok
        with :? Exception as ex ->
            let message = sprintf "Failed to deserialize journal: %s" ex.Message
            Error message
