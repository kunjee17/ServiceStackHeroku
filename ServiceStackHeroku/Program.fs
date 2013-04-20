
// NOTE: If warnings appear, you may need to retarget this project to .NET 4.0. Show the Solution
// Pad, right-click on the project node, choose 'Options --> Build --> General' and change the target
// framework to .NET 4.0 or .NET 4.5.

module ServiceStackHeroku.Main

open System
open ServiceStack
open ServiceStack.ServiceInterface
open ServiceStack.ServiceHost
open ServiceStack.WebHost.Endpoints

[<CLIMutable>]
type HelloResponse = { Result:string }

[<Route("/hello")>]
[<Route("/hello/{name}")>]
type Hello() =
    interface IReturn<HelloResponse>
    member val Name = "" with get, set
    
type HelloService() =
    inherit Service()
    member this.Any (request:Hello) = 
        {Result = "Hello" + request.Name}
        
        
type AppHost() = 
    inherit AppHostHttpListenerBase ("Hello F# Service", typeof<HelloService>.Assembly)
    override this.Configure container = ignore()
    


[<EntryPoint>]
let main args = 
    let host = if args.Length = 0 then "http://*:8080/" else args.[0]
    printfn "listening on %s ..." host
    let appHost = new AppHost()
    appHost.Init()
    appHost.Start host
    Console.ReadLine() |> ignore
    0

