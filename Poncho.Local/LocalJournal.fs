namespace Poncho.Local

module LocalJournal =
    open FsToolkit.ErrorHandling
    open JsonJournal
    open Poncho.Domain.Journal
    open System
    open System.IO

    let saveJournal (journal: Journal) path =
        result {
            let! json = serializeJournal journal

            try
                let path = Path.Combine(path, "poncho.journal.json")
                File.WriteAllText(path, json)
            with :? Exception as ex ->
                let message = sprintf "Failed to save journal: %s" ex.Message
                return! Error message
        }

    let saveIfNotExists (journal: Journal) path =
        match File.Exists(Path.Combine(path, "poncho.journal.json")) with
        | true -> Ok()
        | false -> saveJournal journal path

    let loadJournal path =
        try
            let path = Path.Combine(path, "poncho.journal.json")
            let json = File.ReadAllText(path)
            deserializeJournal json
        with :? Exception as ex ->
            let message = sprintf "Failed to load journal: %s" ex.Message
            Error message

    let backupJournal path =
        let path = Path.Combine(path, "poncho.journal.json")

        let backupPath = Path.Combine(path, "poncho.journal.json.bak")

        File.Copy(path, backupPath, true)

    let restoreJournal path =
        let path = Path.Combine(path, "poncho.journal.json")

        let backupPath = Path.Combine(path, "poncho.journal.json.bak")

        File.Copy(backupPath, path, true)
