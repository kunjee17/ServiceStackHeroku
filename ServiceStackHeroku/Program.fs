
// NOTE: If warnings appear, you may need to retarget this project to .NET 4.0. Show the Solution
// Pad, right-click on the project node, choose 'Options --> Build --> General' and change the target
// framework to .NET 4.0 or .NET 4.5.

module ServiceStackHeroku.Main
open System
open System.Collections.Generic
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
open ServiceStack.OrmLite

//open ServiceStack.OrmLite.Sqlite



//I can't but CLI can mutate this one
[<CLIMutable>]
type HelloResponse = { Result:string }

//There always be hello world, atleast something should be running
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


//This is neat feature. I need to call parameter based constructor first to use 
//member val and then I can give default. So, I dont have to think for default 
//constructor but I have to think about it.
type Rockstar(Id : int, firstName:string, lastName: string, age:int, alive:bool) = 
     member val Id = Id  with get, set
     member val FirstName = firstName with get, set
     member val LastName = lastName with get, set
     member val Age = age with get, set
     member val Alive = alive with get, set
//     member val Url = url with get, set
     member this.Url =  "/stars/{0}/{1}".Fmt(match this.Alive with  
                                                | true -> "alive"
                                                | false -> "dead"
        , this.LastName.ToLower())    
     
     new () = 
        new Rockstar(0,"","",0,true)
        
        
type AppHost() = 
    inherit AppHostHttpListenerBase ("Hello F# Service", typeof<HelloService>.Assembly)
    
 //   static member createUrl (alive:bool, lastname:string) = 
//        AppHost.Instance.Config.WebHostUrl + "/stars/{0}/{1}".Fmt(match alive with  
//                                                                        | true -> "alive"
//                                                                        | false -> "dead"
//                                                                        , lastname.ToLower()); 

    static member SeedData: Rockstar [] =
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
        this.Config.WebHostUrl <- "http://servicestackheroku.herokuapp.com"
        //commented as sqlite dll will not work on linux as dll
//        container.Register<IDbConnectionFactory>(
//            new OrmLiteConnectionFactory(":memory:", false, SqliteDialect.Provider));

        //Nice implementation of using. No more curly braces to give the end
        //commented as sqlite dll will not work on linux as dll
//        use db = container.Resolve<IDbConnectionFactory>().OpenDbConnection()
//        db.CreateTableIfNotExists<Rockstar>();
//        db.InsertAll(AppHost.SeedData); 

        //TODO unable to convert this to F#, do the needful after words
        //SetConfig(new EndpointHostConfig {
        //    CustomHttpHandlers = {
        //        { HttpStatusCode.NotFound, new RazorHandler("/notfound") }
        //    }
        //});

        ignore()
    



        //Rockstars Defined Here     
[<Route("/rockstars")>]
[<Route("/rockstars/{Id}")>]
[<Route("/rockstars/aged/{Age}")>]
type Rockstars (Age:int, Id: int)= 
    //TODO implement nullable age with some-none
    member val Age = Age with get, set
    member val Id = Id with get, set

    new () = new Rockstars (0, 0)

[<Route("/rockstars/delete/{Id}")>]
type DeleteRockStar(Id:int) = 
    member val Id = Id with get, set

//TODO fine a way to skip writing of class and end to define blank class
[<Route("/rockstars/delete/reset")>]
type ResetRockstars() = 
    class 
    end

//TODO aged is nullable implement some none of it
[<Csv(CsvBehavior.FirstEnumerable)>]
type RockstarsResponse(Aged:int, Total:int, Results: List<Rockstar>) =
    member val Total = Total with get, set
    member val Aged = Aged with get, set
    member val Results = Results with get, set 
    
[<ClientCanSwapTemplates>]
[<DefaultView("Rockstars")>]
type RockStarService() =
    inherit Service()

    member this.Get(request:Rockstars) = 
        let rockstarReturn = new List<Rockstar>()
        rockstarReturn.AddRange(AppHost.SeedData)
        let returnResult = match request.Id with 
                                | 0 -> match request.Age with 
                                       | 0 -> rockstarReturn //return all
                                       | _ -> rockstarReturn.FindAll(fun e -> e.Age = request.Age)
                                | _ -> rockstarReturn.FindAll(fun e -> e.Id = request.Id)
        new RockstarsResponse(request.Age, AppHost.SeedData.Length,
            returnResult
        )

    member this.Any(request:DeleteRockStar) = 
        //commented as sqlite dll will not work on linux as dll
        //this.Db.DeleteById<Rockstar>(request.Id)
        this.Get(new Rockstars())

    member this.Post(request: Rockstar) =
        //commented as sqlite dll will not work on linux as dll
        //this.Db.Insert(request)
        this.Get(new Rockstars())

    member this.Any(request: ResetRockstars) = 
        //commented as sqlite dll will not work on linux as dll
        //this.Db.DropAndCreateTable<Rockstar>()
        //this.Db.InsertAll(AppHost.SeedData)
        this.Get(new Rockstars())



[<EntryPoint>]
let main args = 
    LogManager.LogFactory <- new ConsoleLogFactory()
    let env_port = Environment.GetEnvironmentVariable("PORT")
    let port = if env_port = null then "1234" else env_port
    let hostname = "servicestackheroku"
    let host = "http://" + hostname + ".herokuapp.com:" + port + "/"
//    let host = "http://localhost:8080/"
    printfn "listening on %s ..." host
    let appHost = new AppHost()
    appHost.Init()
    appHost.Start host
    while true do Console.ReadLine() |> ignore
    0