module Download

open System
open System.Threading

open FSharp.Data

open Model

type private TickersResponse = JsonProvider<"PolygonResponses/TickersResponse.json", SampleIsList = true>

let rec private requestString (url : string) (query : list<string * string>) =
    let response = Http.Request(url, query, silentHttpErrors = true)
    // Too many requests.
    if response.StatusCode = 429 then
        printfn "Got code 429 with %A" response.Body
        printfn "Waiting for %s" url
        Thread.Sleep(58_000)
        requestString url query
    elif response.StatusCode = 200 then
        match response.Body with
        | Text s -> s
        | Binary _ -> failwithf "Request to %s returned binary data" url
    else
        failwithf "Unexpected code %d on %s" response.StatusCode url

let private downloadRawTickers (apiKey : string) =
    let tickers = ResizeArray()

    let rec go (lastResponse : string) =
        let lastResponse = TickersResponse.Parse lastResponse
        tickers.AddRange(lastResponse.Results)
        match lastResponse.NextUrl with
        | Some nextUrl ->
            let response = requestString nextUrl ["apiKey", apiKey]
            go response
        | None -> ()

    requestString
        "https://api.polygon.io/v3/reference/tickers"
        [ "sort", "ticker"
          "order", "asc"
          "market", "stocks"
          "type", "CS"  // Common Stock.
          "apiKey", apiKey
        ]
    |> go

    tickers.ToArray()

let private convertTicker (t : TickersResponse.Result) = { Ticker = t.Ticker
                                                           Name = t.Name
                                                           Locale = t.Locale
                                                           Currency = t.CurrencyName
                                                           PrimaryExchange = t.PrimaryExchange }

let downloadTickers = downloadRawTickers >> Array.map convertTicker

type private TickerDetailsResponse = JsonProvider<"PolygonResponses/TickerDetailsResponse.json">
type private AggregateBarsResponse = JsonProvider<"PolygonResponses/AggregateBarsResponse.json">

/// Returns all daily aggregated bars for the ticker where the UTC timestamp
/// of the aggregated bar is between `dayFrom` and `dayTo` (inclusive).
let private downloadRawAggregates
    (apiKey : string)
    (ticker : string)
    (dayFrom : DateTime)
    (dayTo : DateTime) =

    let limit = 50_000
    let fmt = "yyyy-MM-dd"
    let strFrom = dayFrom.ToString fmt
    let strTo = dayTo.ToString fmt
    requestString
        $"https://api.polygon.io/v2/aggs/ticker/%s{ticker}/range/1/day/%s{strFrom}/%s{strTo}"
        ["sort", "asc"; "limit", string limit; "adjusted", "true"; "apiKey", apiKey]
    |> AggregateBarsResponse.Parse
    |> fun response -> response.Results

let private convertAggregate (ticker : string) (a : AggregateBarsResponse.Result) =
    { Ticker = ticker
      TimestampSecs = if a.T % 1000L = 0L then a.T / 1000L else failwith $"Unexpected millis %d{a.T}"
      Open = a.O
      Highest = a.H
      Lowest = a.L
      Close = a.C
      PriceWeightedByVolume = Some a.Vw
      Volume = decimal a.V
      NumberOfTransactions = Some a.N }

let downloadAggregates (apiKey : string) (ticker : string) (dayFrom : DateTime) (dayTo : DateTime) =
    downloadRawAggregates apiKey ticker dayFrom dayTo |> Array.map (convertAggregate ticker)
