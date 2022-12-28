namespace Poncho.Cli.DateTime

module NaturalDateTime =
    open Microsoft.Recognizers.Text
    open Microsoft.Recognizers.Text.DateTime
    open System
    open System.Collections.Generic

    let parse dateTime =
        try
            DateTimeRecognizer.RecognizeDateTime(dateTime, Culture.English)
            |> Ok
            |> Result.bind (fun models ->
                match models.Count with
                | 0 -> Error $"No dates found in the '{dateTime}'."
                | _ -> models.Item(0) |> Ok)
            |> Result.bind (fun model ->
                match (model.TypeName.Contains "datetime", model.TypeName.Contains "range") with
                | (true, false) -> model |> Ok
                | _ -> Error $"No dates found in the '{dateTime}'.")
            |> Result.bind (fun model ->
                match model.Resolution["values"] with
                | :? IList<Dictionary<string, string>> as values -> values |> Ok
                | _ -> Error $"No dates found in the '{dateTime}'.")
            |> Result.bind (fun values ->
                match values.Count with
                | 0 -> Error $"No dates found in the '{dateTime}'."
                | _ -> values.Item(0) |> Ok)
            |> Result.bind (fun value ->
                match value.TryGetValue "value" with
                | true, rawDateTime -> (DateTime.TryParse(rawDateTime), rawDateTime) |> Ok
                | _ -> Error $"No dates found in the '{dateTime}'.")
            |> Result.bind (fun (parsed, rawDateTime) ->
                match parsed with
                | (true, dateTime) -> dateTime |> Ok
                | _ -> Error $"Failed to parse the natural date from '{rawDateTime}' and in partical '{rawDateTime}'.")
        with :? Exception ->
            $"Failed to parse the natural date from '{dateTime}'." |> Error
