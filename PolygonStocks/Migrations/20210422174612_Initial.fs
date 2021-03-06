// <auto-generated />
namespace PolygonStocks.Migrations

open System
open Microsoft.EntityFrameworkCore
open Microsoft.EntityFrameworkCore.Infrastructure
open Microsoft.EntityFrameworkCore.Metadata
open Microsoft.EntityFrameworkCore.Migrations
open Microsoft.EntityFrameworkCore.Storage.ValueConversion

[<DbContext(typeof<Model.PolygonContext>)>]
[<Migration("20210422174612_Initial")>]
type Initial() =
    inherit Migration()

    override this.Up(migrationBuilder:MigrationBuilder) =
        migrationBuilder.CreateTable(
            name = "AggregatedBars"
            ,columns = (fun table -> 
            {|
                Ticker =
                    table.Column<string>(
                        nullable = false
                        ,``type`` = "TEXT"
                    )
                TimestampSecs =
                    table.Column<Int64>(
                        nullable = false
                        ,``type`` = "INTEGER"
                    )
                Open =
                    table.Column<decimal>(
                        nullable = false
                        ,``type`` = "TEXT"
                    )
                Highest =
                    table.Column<decimal>(
                        nullable = false
                        ,``type`` = "TEXT"
                    )
                Lowest =
                    table.Column<decimal>(
                        nullable = false
                        ,``type`` = "TEXT"
                    )
                Close =
                    table.Column<decimal>(
                        nullable = false
                        ,``type`` = "TEXT"
                    )
                Volume =
                    table.Column<int>(
                        nullable = false
                        ,``type`` = "INTEGER"
                    )
                PriceWeightedByVolume =
                    table.Column<decimal>(
                        nullable = true
                        ,``type`` = "TEXT"
                    )
                NumberOfTransactions =
                    table.Column<int>(
                        nullable = true
                        ,``type`` = "INTEGER"
                    )
            |})
            ,constraints =
                (fun table -> 
                    table.PrimaryKey("PK_AggregatedBars", (fun x -> (x.Ticker, x.TimestampSecs) :> obj)) |> ignore
                ) 
        ) |> ignore

        migrationBuilder.CreateTable(
            name = "InactiveTickers"
            ,columns = (fun table -> 
            {|
                Ticker =
                    table.Column<string>(
                        nullable = false
                        ,``type`` = "TEXT"
                    )
            |})
            ,constraints =
                (fun table -> 
                    table.PrimaryKey("PK_InactiveTickers", (fun x -> (x.Ticker) :> obj)) |> ignore
                ) 
        ) |> ignore

        migrationBuilder.CreateTable(
            name = "QueuedTickers"
            ,columns = (fun table -> 
            {|
                Ticker =
                    table.Column<string>(
                        nullable = false
                        ,``type`` = "TEXT"
                    )
            |})
            ,constraints =
                (fun table -> 
                    table.PrimaryKey("PK_QueuedTickers", (fun x -> (x.Ticker) :> obj)) |> ignore
                ) 
        ) |> ignore


    override this.Down(migrationBuilder:MigrationBuilder) =
        migrationBuilder.DropTable(
            name = "AggregatedBars"
            ) |> ignore

        migrationBuilder.DropTable(
            name = "InactiveTickers"
            ) |> ignore

        migrationBuilder.DropTable(
            name = "QueuedTickers"
            ) |> ignore


    override this.BuildTargetModel(modelBuilder: ModelBuilder) =
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

