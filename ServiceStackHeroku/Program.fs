
// NOTE: If warnings appear, you may need to retarget this project to .NET 4.0. Show the Solution
// Pad, right-click on the project node, choose 'Options --> Build --> General' and change the target
// framework to .NET 4.0 or .NET 4.5.

module ServiceStackHeroku.Main

open System
open System.Net
open ServiceStack
open ServiceStack.ServiceInterface
open ServiceStack.ServiceHost
open ServiceStack.WebHost.Endpoints
open ServiceStack.Razor
open Funq
open ServiceStack.Text
open ServiceStack.Logging
open ServiceStack.Logging.Support.Logging


type Rockstar(Id : int, firstName:string, lastName: string, age:int, alive:bool) = 
     member val Id = Id  with get, set
     member val firstName = firstName with get, set
     member val lastName = lastName with get, set
     member val age = age with get, set
     member val alive = alive with get, set


     new () = 
        new Rockstar(0,"","",0,true)



[<CLIMutable>]
type HelloResponse = { Result:string }

[<Route("/hello")>]
[<Route("/hello/{name}")>]

type Hello() =
    interface IReturn<HelloResponse>
    member val Name = "" with get, set

[<DefaultView("Hello")>]    
type HelloService() =
    inherit Service()
    member this.Any (request:Hello) = 
        {Result = "Hello" + request.Name}
        
        
type AppHost() = 
    inherit AppHostHttpListenerBase ("Hello F# Service", typeof<HelloService>.Assembly)
    
    static let seedData: Rockstar [] = 
        [|new Rockstar(1, "Jimi", "Hendrix", 27, false); 
            new Rockstar(2, "Janis", "Joplin", 27, false); 
            new Rockstar(4, "Kurt", "Cobain", 27, false);             
            new Rockstar(5, "Elvis", "Presley", 42, false); 
            new Rockstar(6, "Michael", "Jackson", 50, false); 
            new Rockstar(7, "Eddie", "Vedder", 47, true);
            new Rockstar(8, "Dave", "Grohl", 43, true); 
            new Rockstar(9, "Courtney", "Love", 48, true); 
            new Rockstar(10, "Bruce", "Springsteen", 62, true) |]
     
    override this.Configure container = 
        this.Plugins.Add(new RazorFormat())
        ignore()
    
     
    

    

[<EntryPoint>]
let main args = 
    LogManager.LogFactory <- new ConsoleLogFactory()
    let env_port = Environment.GetEnvironmentVariable("PORT")
    let port = if env_port = null then "1234" else env_port
    let hostname = "servicestackheroku"
//    let host = "http://" + hostname + ".herokuapp.com:" + port + "/"
    let host = "http://localhost:8080/"
    printfn "listening on %s ..." host
    let appHost = new AppHost()
    appHost.Init()
    appHost.Start host
    while true do Console.ReadLine() |> ignore
    0