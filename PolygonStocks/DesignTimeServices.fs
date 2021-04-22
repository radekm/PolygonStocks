module DesignTimeServices

open Microsoft.Extensions.DependencyInjection
open Microsoft.EntityFrameworkCore.Design
open EntityFrameworkCore.FSharp

type DesignTimeServices() =
    interface IDesignTimeServices with 
        member _.ConfigureDesignTimeServices(serviceCollection : IServiceCollection) = 
            let fSharpServices = EFCoreFSharpServices() :> IDesignTimeServices
            fSharpServices.ConfigureDesignTimeServices serviceCollection
            ()
