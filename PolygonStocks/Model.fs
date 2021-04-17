module Model

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization

module Serde =
    let private jsonOptions = JsonSerializerOptions()
    do jsonOptions.Converters.Add(JsonFSharpConverter())

    let write (path : string) x =
        let bytes = JsonSerializer.SerializeToUtf8Bytes(x, jsonOptions)
        File.WriteAllBytes(path, bytes)

    let read (path : string) =
        let bytes = File.ReadAllBytes path
        JsonSerializer.Deserialize(ReadOnlySpan bytes, jsonOptions)

type Exchange = { Name : string
                  // Defined by ISO 10383, https://en.wikipedia.org/wiki/Market_Identifier_Code.
                  Mic : string option
                  // Eg. equities, currencies, index.
                  Market : string
                  Tape : string
                  // Used only on Polygon.
                  PolygonCode : string option
                }

type Ticker = { Ticker : string
                Name : string
                Locale : string
                Currency : string
                PrimaryExchange : string }
