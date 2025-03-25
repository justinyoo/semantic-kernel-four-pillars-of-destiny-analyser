var builder = DistributedApplication.CreateBuilder(args);

var config = builder.Configuration;

var aoai = builder.AddConnectionString("aoai");
// var openai = builder.AddConnectionString("openai");

var apiapp = builder.AddProject<Projects.FourPillarsAnalyser_ApiApp>("apiapp")
                    .WithReference(aoai)
                    // .WithReference(openai)
                    .WithEnvironment("Azure__ContainerApps__PoolManagement__Endpoint", config["Azure:ContainerApps:PoolManagement:Endpoint"]!)
                    // .WithEnvironment("Azure__OpenAI__Endpoint", config["Azure:OpenAI:Endpoint"]!)
                    .WithEnvironment("Azure__OpenAI__DeploymentName", config["Azure:OpenAI:DeploymentName"]!)
                    .WithEnvironment("GitHub__Models__ModelId", config["GitHub:Models:ModelId"]!)
                    .WithEnvironment("SemanticKernel__ServiceId", config["SemanticKernel:ServiceId"]!)
                    ;

var webapp = builder.AddProject<Projects.FourPillarsAnalyser_WebApp>("webapp")
                    .WithReference(apiapp)
                    .WaitFor(apiapp);

builder.Build().Run();
