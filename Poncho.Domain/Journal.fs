namespace Poncho.Domain

module Journal =
    open System

    type DoingName = string

    type Doing =
        { name: DoingName
          title: string
          threshold: int
          current: int }

    let isThresholdReached doing = doing.current >= doing.threshold

    type JournalEntry =
        { date: DateOnly
          doings: Doing list
          plan: DoingName list
          commitments: DoingName list
          newDoings: DoingName list
          removedDoings: DoingName list }

    type Metrics = { doingsPerDay: int }

    type Journal =
        { history: JournalEntry list
          metrics: Metrics }

    let emptyJournal =
        { history = []
          metrics = { doingsPerDay = 3 } }

    let lastEntry journal =
        match journal.history with
        | [] -> None
        | entries -> Some(entries |> List.sortBy (fun x -> x.date) |> List.last)

    let initialize journal metrics = { journal with metrics = metrics }

    let daysDifference oneDateMs otherDateMs =
        (oneDateMs - otherDateMs) / 86400000L |> int

    let planDay journal entry =
        { entry with
            plan =
                entry.doings
                |> List.sortByDescending (fun doing -> doing.current)
                |> List.filter (fun doing -> isThresholdReached doing)
                |> List.take journal.metrics.doingsPerDay
                |> List.map (fun doing -> doing.name) }

    let today journal =
        match lastEntry journal with
        | None -> emptyJournal
        | Some(lastEntry) ->
            if lastEntry.date = DateOnly.FromDateTime DateTime.Today then
                journal
            else
                let daysDifference =
                    DateTimeOffset(lastEntry.date.ToDateTime(TimeOnly.MinValue))
                        .ToUnixTimeMilliseconds()
                    |> daysDifference (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())

                let newEntry =
                    { date = DateOnly.FromDateTime DateTime.UtcNow
                      doings =
                        lastEntry.doings
                        |> List.filter (fun doing -> lastEntry.commitments |> List.contains doing.name)
                        |> List.map (fun doing -> { doing with current = 0 })
                        |> List.append
                        <| lastEntry.doings
                        |> List.filter (fun doing -> lastEntry.commitments |> List.contains doing.name |> not)
                        |> List.map (fun doing -> { doing with current = doing.current + daysDifference })
                      plan = []
                      commitments = []
                      newDoings = []
                      removedDoings = [] }

                let plannedNewEntry = planDay journal newEntry

                { journal with history = plannedNewEntry :: journal.history }

    let addDoing journal doing =
        match lastEntry journal with
        | None -> journal
        | Some(entry) ->
            let newEntry =
                { entry with
                    doings = doing :: entry.doings
                    newDoings = doing.name :: entry.newDoings }

            { journal with history = newEntry :: List.filter (fun x -> x.date <> entry.date) journal.history }

    let removeDoing journal doingName =
        match lastEntry journal with
        | None -> journal
        | Some(entry) ->
            let newEntry =
                { entry with
                    doings = entry.doings |> List.filter (fun doing -> doing.name <> doingName)
                    removedDoings = doingName :: entry.removedDoings }

            { journal with history = newEntry :: List.filter (fun x -> x.date <> entry.date) journal.history }

    let skip journal doingName =
        match lastEntry journal with
        | None -> journal
        | Some(entry) ->
            let newEntry =
                { entry with commitments = entry.commitments |> List.filter (fun name -> name <> doingName) }

            { journal with history = newEntry :: List.filter (fun x -> x.date <> entry.date) journal.history }

    let replace journal replacableDoing newCommitment =
        match lastEntry journal with
        | None -> journal
        | Some(entry) ->
            let newEntry =
                { entry with
                    commitments =
                        entry.commitments
                        |> List.map (fun doing -> if doing = replacableDoing then newCommitment else doing) }

            { journal with history = newEntry :: List.filter (fun x -> x.date <> entry.date) journal.history }
