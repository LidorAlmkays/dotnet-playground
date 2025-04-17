var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.AuthService>("AuthService")
                .WithEndpoint("http", endpoint => endpoint.IsProxied = false)
                 .WithEndpoint("https", endpoint => endpoint.IsProxied = false);

builder.Build().Run();
