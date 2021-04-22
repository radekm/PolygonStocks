﻿// <auto-generated />
namespace PolygonStocks.Migrations

open System
open Microsoft.EntityFrameworkCore
open Microsoft.EntityFrameworkCore.Infrastructure
open Microsoft.EntityFrameworkCore.Metadata
open Microsoft.EntityFrameworkCore.Migrations
open Microsoft.EntityFrameworkCore.Storage.ValueConversion

[<DbContext(typeof<Model.PolygonContext>)>]
type PolygonContextModelSnapshot() =
    inherit ModelSnapshot()

    override this.BuildModel(modelBuilder: ModelBuilder) =
        modelBuilder
            .HasAnnotation("ProductVersion", "5.0.5")
            |> ignore

        modelBuilder.Entity("Model+AggregatedBar", (fun b ->

            b.Property<string>("Ticker")
                .HasColumnType("TEXT") |> ignore
            b.Property<Int64>("TimestampSecs")
                .HasColumnType("INTEGER") |> ignore
            b.Property<decimal>("Close")
                .IsRequired()
                .HasColumnType("TEXT") |> ignore
            b.Property<decimal>("Highest")
                .IsRequired()
                .HasColumnType("TEXT") |> ignore
            b.Property<decimal>("Lowest")
                .IsRequired()
                .HasColumnType("TEXT") |> ignore
            b.Property<int>("NumberOfTransactions")
                .HasColumnType("INTEGER") |> ignore
            b.Property<decimal>("Open")
                .IsRequired()
                .HasColumnType("TEXT") |> ignore
            b.Property<decimal>("PriceWeightedByVolume")
                .HasColumnType("TEXT") |> ignore
            b.Property<int>("Volume")
                .IsRequired()
                .HasColumnType("INTEGER") |> ignore

            b.HasKey("Ticker", "TimestampSecs") |> ignore

            b.ToTable("AggregatedBars") |> ignore

        )) |> ignore

        modelBuilder.Entity("Model+InactiveTicker", (fun b ->

            b.Property<string>("Ticker")
                .HasColumnType("TEXT") |> ignore

            b.HasKey("Ticker") |> ignore

            b.ToTable("InactiveTickers") |> ignore

        )) |> ignore

        modelBuilder.Entity("Model+QueuedTicker", (fun b ->

            b.Property<string>("Ticker")
                .HasColumnType("TEXT") |> ignore

            b.HasKey("Ticker") |> ignore

            b.ToTable("QueuedTickers") |> ignore

        )) |> ignore
