module Download

open System
open System.Threading

open FSharp.Data

open Model

type private ExchangesResponse = JsonProvider<"PolygonResponses/ExchangesResponse.json">     

let private downloadRawExchanges (apiKey : string) =
    Http.RequestString("https://api.polygon.io/v1/meta/exchanges", ["apiKey", apiKey])
    |> ExchangesResponse.Parse
    |> Array.filter (fun e -> e.Type = "exchange")

let private convertExchange (e : ExchangesResponse.Root) = { Name = e.Name
                                                             Mic = e.Mic
                                                             Market = e.Market
                                                             Tape = e.Type
                                                             PolygonCode = e.Code }

let downloadExchanges = downloadRawExchanges >> Array.map convertExchange

type private TickersResponse = JsonProvider<"PolygonResponses/TickersResponse.json">     

let private downloadRawTickers (apiKey : string) =
    let rec go (acc : ResizeArray<_>) (page : int) =
        let parsed =
            Http.RequestString(
                "https://api.polygon.io/v2/reference/tickers",
                [ "sort", "ticker"
                  "perpage", "50"; "page", string page
                  "market", "STOCKS"; "active", "true"
                  "apiKey", apiKey
                ])
            |> TickersResponse.Parse
        printfn "Downloaded page %d with %d tickers of %d, first ticker %A"
            page parsed.Tickers.Length parsed.Count
            (parsed.Tickers |> Array.tryHead |> Option.map (fun t -> t.Ticker))
        parsed.Tickers |> acc.AddRange
        if parsed.Tickers.Length >= parsed.PerPage
        then go acc (page + 1)
        else acc.ToArray()

    go (ResizeArray()) 1

let private convertTicker (t : TickersResponse.Ticker) = { Ticker = t.Ticker
                                                           Name = t.Name
                                                           Locale = t.Locale
                                                           Currency = t.Currency
                                                           PrimaryExchange = t.PrimaryExch }

let downloadTickers = downloadRawTickers >> Array.map convertTicker

type private TickerDetailsResponse = JsonProvider<"PolygonResponses/TickerDetailsResponse.json">     
type private AggregateBarsResponse = JsonProvider<"PolygonResponses/AggregateBarsResponse.json">     

/// Returns all aggregated bars for the ticker where the UTC timestamp
/// of the aggregated bar is between `dayFrom` and `dayTo` (inclusive).
let private downloadRawAggregates (apiKey : string) (ticker : string) (dayFrom : DateTime) (dayTo : DateTime) =
    let limit = 50_000
    let result = ResizeArray()
    
    // Returns a number of requests which were used to download aggregated bars.
    let rec download (dayFrom : DateTime) (dayTo : DateTime) nFailures nRequests : int =
        let response =
            let fmt = "yyyy-MM-dd"
            // Make the interval slightly bigger because we don't know if Polygon's REST API uses
            // dates in UTC or some other time zone (eg. time zone of the exchange).
            let strFrom = dayFrom.AddDays(-1.0).ToString fmt
            let strTo = dayTo.AddDays(1.0).ToString fmt
            Http.Request(
                $"https://api.polygon.io/v2/aggs/ticker/%s{ticker}/range/1/minute/%s{strFrom}/%s{strTo}",
                ["sort", "asc"; "limit", string limit; "unadjusted", "true"; "apiKey", apiKey],
                silentHttpErrors = true)
        let body =
            match response.Body with
            | Text s -> s
            | Binary _ -> failwith $"Unexpected binary response when downloading ticker %s{ticker}"

        if response.StatusCode = 200 then
            let parsed = AggregateBarsResponse.Parse body
            
            result.AddRange parsed.Results

            // We probably don't have all aggregated bars.
            if parsed.Results.Length >= limit then
                let lastDayInResults =
                    parsed.Results
                    |> Array.maxBy (fun a -> a.T)
                    |> fun a -> DateTimeOffset.FromUnixTimeMilliseconds(a.T).Date

                download lastDayInResults dayTo nFailures (nRequests + 1)
            else nRequests + 1
        elif response.StatusCode = 429 then
            printfn $"Too many requests per minute (%d{nFailures}). Waiting..."
            Thread.Sleep 15_000
            download dayFrom dayTo (nFailures + 1) nRequests
        else failwith $"Unexpected HTTP code %d{response.StatusCode} when downloading ticker %s{ticker}, body: %s{body}"

    let nRequests = download dayFrom dayTo 0 0

    let removeDuplicatesByTimestamp (aggregates : AggregateBarsResponse.Result[]) =
        let mutable lastTs = Int64.MinValue
        aggregates
        |> Array.filter (fun a ->
            let origLastTimestamp = lastTs
            lastTs <- a.T
            a.T <> origLastTimestamp)

    let result =
        let tsFrom = DateTimeOffset(dayFrom.Date, TimeSpan.Zero).ToUnixTimeMilliseconds()
        let tsTo = DateTimeOffset(dayTo.AddDays(1.0).Date, TimeSpan.Zero).ToUnixTimeMilliseconds()
        result.ToArray()
        |> Array.filter (fun a -> tsFrom <= a.T && a.T < tsTo)
        |> Array.sortBy (fun a -> a.T)
        |> removeDuplicatesByTimestamp

    printfn $"Downloaded %d{result.Length} aggregates of %s{ticker} in %d{nRequests} requests"            

    result

let private convertAggregate (ticker : string) (a : AggregateBarsResponse.Result) =
    { Ticker = ticker
      TimestampSecs = if a.T % 1000L = 0L then a.T / 1000L else failwith $"Unexpected millis %d{a.T}" 
      Open = a.O
      Highest = a.H
      Lowest = a.L
      Close = a.C
      PriceWeightedByVolume = a.Vw
      Volume = a.V
      NumberOfTransactions = a.N }

let downloadAggregates (apiKey : string) (ticker : string) (dayFrom : DateTime) (dayTo : DateTime) =
    downloadRawAggregates apiKey ticker dayFrom dayTo |> Array.map (convertAggregate ticker)
