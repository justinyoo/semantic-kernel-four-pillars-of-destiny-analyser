using System.Reflection;

using Azure.Identity;

using FourPillarsAnalyser.ApiApp.Delegates;
using FourPillarsAnalyser.ApiApp.Endpoints;
using FourPillarsAnalyser.ApiApp.Plugins.PersonalDetails;
using FourPillarsAnalyser.ApiApp.Services;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter;

using OpenAI;

using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults();

builder.Services.AddOpenApi();

builder.Services.AddScoped<IKernelService, KernelService>();

builder.AddKeyedAzureOpenAIClient("aoai");
// builder.AddKeyedAzureOpenAIClient("openai");

builder.Services.AddSingleton<Kernel>(sp =>
{
    var config = builder.Configuration;

    var aoaiClient = sp.GetRequiredKeyedService<OpenAIClient>("aoai");
    // var openAIClient = sp.GetRequiredKeyedService<OpenAIClient>("openai");

    var kb = Kernel.CreateBuilder()
                   .AddOpenAIChatCompletion(
                       modelId: config["Azure:OpenAI:DeploymentName"]!,
                       openAIClient: aoaiClient,
                       serviceId: "azure")
                //    .AddOpenAIChatCompletion(
                //        modelId: config["GitHub:Models:ModelId"]!,
                //        openAIClient: openAIClient,
                //        serviceId: "github")
                    ;
    kb.Services.AddHttpClient()
               .AddSingleton<SessionsPythonPlugin>(sp => DependencyDelegates.GetSessionsPythonPluginAsync(sp, config));
    
    var rb = ResourceBuilder.CreateDefault()
                            .AddService(config["OTEL_SERVICE_NAME"]!);
    kb.Services.AddOpenTelemetry()
               .WithMetrics(mpb => mpb.AddMeter("Microsoft.SemanticKernel*")
                                      .SetResourceBuilder(rb)
                                    //   .ConfigureResource(rb => rb.AddService(config["OTEL_SERVICE_NAME"]!))
                                      .AddConsoleExporter()
                                      .AddOtlpExporter(o => o.Endpoint = new Uri(config["OTEL_EXPORTER_OTLP_ENDPOINT"]!)))
               .WithTracing(tpb => tpb.AddSource("Microsoft.SemanticKernel*")
                                      .SetResourceBuilder(rb)
                                    //   .ConfigureResource(rb => rb.AddService(config["OTEL_SERVICE_NAME"]!))
                                      .AddConsoleExporter()
                                      .AddOtlpExporter(o => o.Endpoint = new Uri(config["OTEL_EXPORTER_OTLP_ENDPOINT"]!)))
               .WithLogging(lpb => lpb.SetResourceBuilder(rb)
                                    //   .ConfigureResource(rb => rb.AddService(config["OTEL_SERVICE_NAME"]!))
                                      .AddConsoleExporter()
                                      .AddOtlpExporter(o => o.Endpoint = new Uri(config["OTEL_EXPORTER_OTLP_ENDPOINT"]!)),
                             options =>
                             {
                                options.IncludeFormattedMessage = true;
                                options.IncludeScopes = true;
                             })
               ;

    var kernel = kb.Build();

    // var pluginpath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Plugins");
    // var plugins = kernel.CreatePluginFromPromptDirectory(pluginpath);
    // kernel.Plugins.Add(plugins);
    kernel.Plugins.AddFromObject(kernel.GetRequiredService<SessionsPythonPlugin>());
    // kernel.Plugins.AddFromType<PersonalDetailsPlugin>();

    return kernel;
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapChatCompletionEndpoint();

app.Run();
