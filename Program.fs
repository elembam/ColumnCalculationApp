open System
open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Newtonsoft.Json

// Define a record type to hold the data with a Shape field
type ColumnRecord = {
    Mantel: string
    Kern: string
    Length: int
    FireRating: string
    Load: float
}

let initialData = [
    { Mantel = "100x100"; Kern = "50"; Length = 2000; FireRating = "R0"; Load = 659.0 }
    { Mantel = "100x100"; Kern = "50"; Length = 2000; FireRating = "R30"; Load = 237.0 }
    { Mantel = "100x100"; Kern = "50"; Length = 2000; FireRating = "R60"; Load = 70.0 }
    { Mantel = "100x100"; Kern = "50"; Length = 2000; FireRating = "R90"; Load = 36.0 }
    { Mantel = "100x100"; Kern = "50"; Length = 2250; FireRating = "R0"; Load = 596.0 }
    { Mantel = "100x100"; Kern = "50"; Length = 2250; FireRating = "R30"; Load = 194.0 }
    { Mantel = "100x100"; Kern = "50"; Length = 2250; FireRating = "R60"; Load = 57.0 }
    { Mantel = "100x100"; Kern = "50"; Length = 2250; FireRating = "R90"; Load = 0.0 }
]

// Function to find the closest load values to the user input
let findClosestRecords (userInputLoad: float) (userInputLength: int) (data: ColumnRecord list) =
    let filteredData = data |> List.filter (fun record -> record.Length = userInputLength)
    let sortedData = filteredData |> List.sortBy (fun record -> Math.Abs(record.Load - userInputLoad))
    sortedData |> List.take 3

// Function to display records as a string
let displayRecords (records: ColumnRecord list) =
    records |> List.map (fun record ->
        sprintf "Mantel: %s, Kern: %s, Length: %d, FireRating: %s, Load: %f"
                record.Mantel record.Kern record.Length record.FireRating record.Load
    ) |> String.concat "<br>"

// Define the web app
let webApp (logger: ILogger) =
    choose [
        route "/" >=> htmlFile "wwwroot/index.html"
        route "/query" >=> fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let loadValue = ctx.Request.Query.["load"]
                let lengthValue = ctx.Request.Query.["length"]
                logger.LogInformation($"Received parameters: loadValue={loadValue}, lengthValue={lengthValue}")

                match Double.TryParse(loadValue), Int32.TryParse(lengthValue) with
                | (true, userInputLoad), (true, userInputLength) ->
                    let closestRecords = findClosestRecords userInputLoad userInputLength initialData
                    let result = displayRecords closestRecords
                    logger.LogInformation($"Query result: {result}")
                    return! htmlString result next ctx
                | _ ->
                    logger.LogInformation("Invalid input.")
                    let errorResult = "<p>Invalid input. Please enter valid load and length values.</p>"
                    return! htmlString errorResult next ctx
            }
    ]

// Configure services
let configureServices (services: IServiceCollection) =
    services.AddGiraffe() |> ignore
    services.AddLogging(fun builder ->
        builder.AddConsole() |> ignore
        builder.AddDebug() |> ignore) |> ignore

// Configure the HTTP request pipeline
let configureApp (app: IApplicationBuilder) =
    let logger = app.ApplicationServices.GetService<ILogger<obj>>()
    app.UseStaticFiles()
       .UseGiraffe(webApp logger)

// Configure and run the web host
[<EntryPoint>]
let main argv =
    Host.CreateDefaultBuilder(argv)
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .ConfigureServices(configureServices)
                .Configure(configureApp)
                .UseUrls("http://localhost:5000") |> ignore)
        .Build()
        .Run()
    0
