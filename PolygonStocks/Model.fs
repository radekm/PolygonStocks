module Model

open System.ComponentModel.DataAnnotations

open Microsoft.EntityFrameworkCore
open EntityFrameworkCore.FSharp

[<CLIMutable>]
type Exchange = { [<Key>] Name : string
                  // Defined by ISO 10383, https://en.wikipedia.org/wiki/Market_Identifier_Code.
                  Mic : string option
                  // Eg. equities, currencies, index.
                  Market : string
                  Tape : string
                  // Used only on Polygon.
                  PolygonCode : string option
                }

[<CLIMutable>]
type Ticker = { [<Key>] Ticker : string
                Name : string
                Locale : string
                Currency : string
                PrimaryExchange : string }

[<CLIMutable>]
type AggregatedBar = { Ticker : string
                       // `int64` instead of `DateTimeOffset` because EF with SQLite
                       // is unable to use `DateTimeOffset` in comparison.
                       TimestampSecs : int64
                       Open : decimal
                       Highest : decimal
                       Lowest : decimal
                       Close : decimal
                       Volume : int                       
                       PriceWeightedByVolume : decimal option
                       NumberOfTransactions : int option
                     }

[<CLIMutable>]
type InactiveTicker = { [<Key>] Ticker : string }

[<CLIMutable>]
type QueuedTicker = { [<Key>] Ticker : string }

type PolygonContext() =  
    inherit DbContext()

    [<DefaultValue>] val mutable exchanges : DbSet<Exchange>
    member me.Exchanges with get() = me.exchanges and set v = me.exchanges <- v

    [<DefaultValue>] val mutable tickers : DbSet<Ticker>
    member me.Tickers with get() = me.tickers and set v = me.tickers <- v
    
    [<DefaultValue>] val mutable aggregatedBars : DbSet<AggregatedBar>
    member me.AggregatedBars with get() = me.aggregatedBars and set v = me.aggregatedBars <- v

    [<DefaultValue>] val mutable inactiveTickers : DbSet<InactiveTicker>
    member me.InactiveTickers
        with get() = me.inactiveTickers
        and set v = me.inactiveTickers <- v

    [<DefaultValue>] val mutable queuedTickers : DbSet<QueuedTicker>
    member me.QueuedTickers
        with get() = me.queuedTickers
        and set v = me.queuedTickers <- v

    override _.OnModelCreating(mb) =
        let entity = mb.Entity<Exchange>()
        entity
            .Property(fun e -> e.Mic)
            .HasConversion(OptionConverter())
        |> ignore
        entity
            .Property(fun e -> e.PolygonCode)
            .HasConversion(OptionConverter())
        |> ignore

        let entity = mb.Entity<AggregatedBar>()
        entity
            .HasKey(fun a -> (a.Ticker, a.TimestampSecs) :> obj)
        |> ignore
        entity
            .Property(fun a -> a.PriceWeightedByVolume)
            .HasConversion(OptionConverter())
        |> ignore
        entity
            .Property(fun a -> a.NumberOfTransactions)
            .HasConversion(OptionConverter())
        |> ignore

    override _.OnConfiguring(options : DbContextOptionsBuilder) =
        options.UseSqlite("Data Source=Stocks.db") |> ignore
