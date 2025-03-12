var builder = DistributedApplication.CreateBuilder(args);

var config = builder.Configuration;

var openai = builder.AddConnectionString("openai");

var apiapp = builder.AddProject<Projects.FourPillarsAnalyser_ApiApp>("apiapp")
                    .WithReference(openai)
                    .WithEnvironment("SemanticKernel__ServiceId", config["SemanticKernel:ServiceId"]!)
                    .WithEnvironment("GitHub__Models__ModelId", config["GitHub:Models:ModelId"]!);

var webapp = builder.AddProject<Projects.FourPillarsAnalyser_WebApp>("webapp")
                    .WithReference(apiapp)
                    .WaitFor(apiapp);

builder.Build().Run();
