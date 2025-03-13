using Azure.Identity;

using FourPillarsAnalyser.ApiApp.Delegates;
using FourPillarsAnalyser.ApiApp.Endpoints;
using FourPillarsAnalyser.ApiApp.Plugins.PersonalDetails;
using FourPillarsAnalyser.ApiApp.Services;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter;

using OpenAI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults();

builder.Services.AddOpenApi();

builder.Services.AddScoped<IKernelService, KernelService>();

builder.AddAzureOpenAIClient("openai");

builder.Services.AddSingleton<Kernel>(sp =>
{
    var config = builder.Configuration;

    var openAIClient = sp.GetRequiredService<OpenAIClient>();

    var kb = Kernel.CreateBuilder()
                   .AddOpenAIChatCompletion(
                       modelId: config["GitHub:Models:ModelId"]!,
                       openAIClient: openAIClient,
                       serviceId: "github");
    kb.Services.AddHttpClient()
               .AddSingleton<SessionsPythonPlugin>(sp => DependencyDelegates.GetSessionsPythonPluginAsync(sp, config));

    var kernel = kb.Build();

    kernel.Plugins.AddFromObject(kernel.GetRequiredService<SessionsPythonPlugin>());
    kernel.Plugins.AddFromType<PersonalDetailsPlugin>();

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
