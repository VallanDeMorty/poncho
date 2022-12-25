namespace Poncho.Domain

module Journal =
    open System

    type DoingName = string

    [<CustomEquality; NoComparison>]
    type Doing =
        { name: DoingName
          title: string
          threshold: int
          current: int }

        override this.Equals other =
            match other with
            | :? Doing as d -> d.name.Equals this.name
            | _ -> false

        override this.GetHashCode() = this.name.GetHashCode()

    let isThresholdReached doing = doing.current >= doing.threshold

    let verifyDoing doing =
        let errors =
            [ if doing.name.Length = 0 then
                  Some("Name cannot be empty")
              else
                  None
              if doing.title.Length = 0 then
                  Some("Title cannot be empty")
              else
                  None
              if doing.threshold <= 0 then
                  Some("Threshold must be greater than 0")
              else
                  None
              if doing.current < 0 then
                  Some("Current must be greater than or equal to 0")
              else
                  None ]

        match errors |> List.filter Option.isSome with
        | [] -> Ok(doing)
        | _ ->
            errors
            |> List.choose id
            |> List.fold (fun message error -> $"{message}{error}. ") ""
            |> Error

    type JournalEntry =
        { date: DateOnly
          doings: Doing list
          plan: DoingName list
          commitments: DoingName list
          newDoings: DoingName list
          removedDoings: DoingName list }

    let isEntryToday entry =
        entry.date = DateOnly.FromDateTime DateTime.Now

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

    let lastEntryAsResult journal =
        match lastEntry journal with
        | None -> Error("No entries found in journal.")
        | Some(entry) -> Ok(entry)

    let initialize journal metrics = { journal with metrics = metrics }

    let daysDifference oneDateMs otherDateMs =
        (oneDateMs - otherDateMs) / 86400000L |> int

    let private planDay journal entry =
        { entry with
            plan =
                entry.doings
                |> List.sortByDescending (fun doing -> doing.current)
                |> List.filter (fun doing -> isThresholdReached doing)
                |> List.take journal.metrics.doingsPerDay
                |> List.map (fun doing -> doing.name) }

    let today journal =
        match lastEntry journal with
        | None ->
            { journal with
                history =
                    [ { date = DateOnly.FromDateTime DateTime.Today
                        doings = []
                        plan = []
                        commitments = []
                        newDoings = []
                        removedDoings = [] } ] }
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
        lastEntryAsResult journal
        |> Result.bind (fun lastEntry ->
            match
                lastEntry.doings
                |> List.tryFind (fun presentDoing -> presentDoing.name = doing.name)
            with
            | Some(_) -> Error($"Doing {doing.name} is already present.")
            | _ -> Ok(lastEntry))
        |> Result.map (fun lastEntry ->
            let newEntry =
                { lastEntry with
                    doings = doing :: lastEntry.doings
                    newDoings = doing.name :: lastEntry.newDoings }

            { journal with history = newEntry :: List.filter (fun x -> x.date <> lastEntry.date) journal.history })

    let removeDoing journal doingName =
        lastEntryAsResult journal
        |> Result.bind (fun lastEntry ->
            match lastEntry.doings |> List.tryFind (fun doing -> doing.name = doingName) with
            | None -> Error($"Doing {doingName} is not present.")
            | _ -> Ok(lastEntry))
        |> Result.map (fun lastEntry ->
            match lastEntry.newDoings |> List.tryFind (fun name -> name = doingName) with
            | Some(_) ->
                { lastEntry with
                    doings = lastEntry.doings |> List.filter (fun doing -> doing.name <> doingName)
                    newDoings = lastEntry.newDoings |> List.filter (fun name -> name <> doingName) }
            | _ ->
                { lastEntry with
                    doings = lastEntry.doings |> List.filter (fun doing -> doing.name <> doingName)
                    removedDoings = doingName :: lastEntry.removedDoings })
        |> Result.map (fun newEntry ->
            { journal with history = newEntry :: List.filter (fun x -> x.date <> newEntry.date) journal.history })

    let skip journal doingName =
        lastEntryAsResult journal
        |> Result.bind (fun lastEntry ->
            match lastEntry.doings |> List.tryFind (fun doing -> doing.name = doingName) with
            | None -> Error($"Doing {doingName} is not present.")
            | _ -> Ok(lastEntry))
        |> Result.map (fun lastEntry ->
            let newEntry =
                { lastEntry with commitments = lastEntry.commitments |> List.filter (fun name -> name <> doingName) }

            { journal with history = newEntry :: List.filter (fun x -> x.date <> lastEntry.date) journal.history })

    let replace journal replacableDoing newCommitment =
        lastEntryAsResult journal
        |> Result.bind (fun lastEntry ->
            let doingErrors =
                [ if lastEntry.doings |> List.exists (fun doing -> doing.name = replacableDoing) then
                      None
                  else
                      Some($"Doing {replacableDoing} is not present")
                  if lastEntry.doings |> List.exists (fun doing -> doing.name = newCommitment) then
                      None
                  else
                      Some($"Doing {newCommitment} is not present") ]

            match doingErrors |> List.filter Option.isSome with
            | [] -> Ok lastEntry
            | errors -> errors |> List.map Option.get |> String.concat ". " |> Error)
        |> Result.map (fun lastEntry ->
            let newEntry =
                { lastEntry with
                    commitments =
                        lastEntry.commitments
                        |> List.map (fun doing -> if doing = replacableDoing then newCommitment else doing) }

            { journal with history = newEntry :: List.filter (fun x -> x.date <> lastEntry.date) journal.history })
