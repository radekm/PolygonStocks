module Program

open System

open Model

// Returns queued tickers already attached to `ctx`.
let fetchQueuedTickers (ctx : PolygonContext) =
    let queued = ctx.QueuedTickers |> Seq.toArray
    if queued |> Array.isEmpty |> not
    then queued
    else
        // Queue is empty.
        // Let's fill it with all active tickers.
        let allTickers = ctx.Tickers |> Seq.map (fun t -> t.Ticker) |> Seq.toArray
        let inactiveTickers = ctx.InactiveTickers |> Seq.map (fun t -> t.Ticker) |> Set.ofSeq
        let queued = 
            allTickers
            |> Array.filter (fun t -> Set.contains t inactiveTickers |> not)
            |> Array.map (fun t -> { Ticker = t })

        queued |> Array.iter (ctx.QueuedTickers.Add >> ignore)
        ctx.SaveChanges() |> ignore

        queued

[<EntryPoint>]
let main argv =
    let apiKey = argv.[0]
    
    use ctx = new PolygonContext()

    match argv.[1] with
    | "download-tickers" ->
        ctx.Tickers.RemoveRange(ctx.Tickers)
        Download.downloadTickers apiKey |> ctx.Tickers.AddRange
        ctx.SaveChanges() |> ignore
    | "download-aggregates" ->
        let fromDay, toDay =
            let toDay = DateTime.UtcNow.Date
            let fromDay = DateTime(toDay.Year, toDay.Month, 1).AddMonths(-40).Date
            fromDay, toDay
        printfn $"To download %A{fromDay} - %A{toDay}"

        let queued = fetchQueuedTickers ctx
        printfn "Found %d queued tickers" queued.Length
        
        for qt in queued do
            let lastTimestampSecs =                
                query {
                    for a in ctx.AggregatedBars do
                    where (qt.Ticker = a.Ticker)
                    sortByDescending a.TimestampSecs
                    select a.TimestampSecs
                    take 1
                } |> Seq.tryHead

            let timestampPredicate, fromDay =
                match lastTimestampSecs with
                | Some lastTimestampSecs ->
                    (fun ts -> lastTimestampSecs < ts), DateTimeOffset.FromUnixTimeSeconds(lastTimestampSecs).Date
                | None -> (fun _ -> true), fromDay
            
            printfn "Downloading ticker %s from %s" qt.Ticker (fromDay.ToString "yyyy-MM-dd")
            let downloadedAggregates = Download.downloadAggregates apiKey qt.Ticker fromDay toDay
            let aggregatesToWrite = downloadedAggregates |> Array.filter (fun a -> timestampPredicate a.TimestampSecs)
            printfn "Writing %d aggregates" aggregatesToWrite.Length
            aggregatesToWrite |> Array.iter (ctx.AggregatedBars.Add >> ignore)            
            ctx.QueuedTickers.Remove(qt) |> ignore            
            ctx.SaveChanges() |> ignore

            // Mark the ticker as inactive if it has no aggregated bar in last two weeks
            let lastTwoWeeksTimestamp = DateTimeOffset(toDay.AddDays(-14.0), TimeSpan.Zero).ToUnixTimeSeconds()
            let recentActivity =
                query {
                    for a in ctx.aggregatedBars do
                    exists (a.Ticker = qt.Ticker && a.TimestampSecs >= lastTwoWeeksTimestamp)
                }
            if not recentActivity then
                printfn "Marking %s as inactive" qt.Ticker
                ctx.InactiveTickers.Add { Ticker = qt.Ticker }
                |> ignore
                ctx.SaveChanges() |> ignore

            // Since `ctx` is long-lived we have to discard tracked entities manually.
            ctx.ChangeTracker.Clear()

    | cmd -> failwithf $"Unknown command: %s{cmd}"

    0
