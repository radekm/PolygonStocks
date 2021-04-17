module Program

open System

open Model

[<EntryPoint>]
let main argv =
    let apiKey = argv.[0]
    
    match argv.[1] with
    | "download-exchanges" ->
        Download.downloadExchanges apiKey
        |> Serde.write "Data/exchanges.json"
    | "download-tickers" ->
        Download.downloadTickers apiKey
        |> Serde.write "Data/tickers.json"
    | cmd -> failwithf $"Unknown command: %s{cmd}"

    0
