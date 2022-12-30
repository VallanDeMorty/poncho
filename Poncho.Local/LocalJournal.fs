namespace Poncho.Local

module LocalJournal =
    open FsToolkit.ErrorHandling
    open JsonJournal
    open Poncho.Domain.Journal
    open System
    open System.IO

    let private journalFileName = "poncho.journal.json"

    let private backupJournalFileName = "poncho.journal.json.bak"

    let previewJournalPath path = Path.Combine(path, journalFileName)

    let saveJournal (journal: Journal) path =
        result {
            let! json = serializeJournal journal

            try
                let path = Path.Combine(path, journalFileName)
                File.WriteAllText(path, json)
            with :? Exception as ex ->
                let message = sprintf "Failed to save journal: %s" ex.Message
                return! Error message
        }

    let saveIfNotExists (journal: Journal) path =
        match File.Exists(Path.Combine(path, journalFileName)) with
        | true -> Ok()
        | false -> saveJournal journal path

    let loadJournal path =
        try
            let path = Path.Combine(path, journalFileName)
            let json = File.ReadAllText(path)
            deserializeJournal json
        with :? Exception as ex ->
            let message = sprintf "Failed to load journal: %s" ex.Message
            Error message

    let backupJournal path =
        let path = Path.Combine(path, journalFileName)

        let backupPath = Path.Combine(path, backupJournalFileName)

        File.Copy(path, backupPath, true)

    let restoreJournal path =
        let path = Path.Combine(path, journalFileName)

        let backupPath = Path.Combine(path, backupJournalFileName)

        File.Copy(backupPath, path, true)
