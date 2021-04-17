module Download

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
