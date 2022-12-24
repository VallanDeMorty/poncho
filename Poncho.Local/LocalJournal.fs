﻿namespace Poncho.Local

module LocalJournal =
    open FSharp.Json
    open System.IO
    open Poncho.Domain.Journal
    open System
    open FsToolkit.ErrorHandling

    let serializeJournal journal =
        try
            Json.serialize journal |> Ok
        with :? Exception as ex ->
            let message = sprintf "Failed to serialize journal: %s" ex.Message
            Error message

    let saveJournal journal path =
        result {
            let! json = serializeJournal journal

            try
                let path = Path.Combine(path, "poncho.journal.json")
                File.WriteAllText(path, json)
            with :? Exception as ex ->
                let message = sprintf "Failed to save journal: %s" ex.Message
                return! Error message
        }

    let deserializeJournal json =
        try
            Json.deserialize<Journal> json |> Ok
        with :? Exception as ex ->
            let message = sprintf "Failed to deserialize journal: %s" ex.Message
            Error message

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