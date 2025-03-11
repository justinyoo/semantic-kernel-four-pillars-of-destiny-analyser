var builder = DistributedApplication.CreateBuilder(args);

var apiapp = builder.AddProject<Projects.FourPillarsAnalyser_ApiApp>("apiapp");

var webapp = builder.AddProject<Projects.FourPillarsAnalyser_WebApp>("webapp")
                    .WithReference(apiapp)
                    .WaitFor(apiapp);

builder.Build().Run();
