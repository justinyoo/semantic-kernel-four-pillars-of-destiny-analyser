using System.Reflection;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

using ChatMessageContent = Microsoft.SemanticKernel.ChatMessageContent;

namespace FourPillarsAnalyser.ApiApp.Services;

public interface IKernelService
{
    IAsyncEnumerable<string> CompleteChatStreamingAsync(IEnumerable<ChatMessageContent> messages);
}

public class KernelService(Kernel kernel, IConfiguration config) : IKernelService
{
    public async IAsyncEnumerable<string> CompleteChatStreamingAsync(IEnumerable<ChatMessageContent> messages)
    {
        // await foreach (var text in this.InvokeChatMessageContentsAsync(messages))
        await foreach (var text in this.InvokeFortuneTellerAgentAsync(messages))
        {
            yield return text;
        }
    }

    private async IAsyncEnumerable<string> InvokeChatMessageContentsAsync(IEnumerable<ChatMessageContent> messages)
    {
        var history = new ChatHistory();
        history.AddRange(messages);

        var service = kernel.GetRequiredService<IChatCompletionService>();
        var settings = new PromptExecutionSettings()
        {
            ServiceId = config["SemanticKernel:ServiceId"],
            // FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var result = service.GetStreamingChatMessageContentsAsync(chatHistory: history, executionSettings: settings, kernel: kernel);
        await foreach (var text in result)
        {
            yield return text.ToString();
        }
    }

    private async IAsyncEnumerable<string> InvokeFortuneTellerAgentAsync(IEnumerable<ChatMessageContent> messages)
    {
        var filepath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                                    "Agents",
                                    "FortuneTellerAgent",
                                    "FortuneTeller.ko.yaml");
        var definition = File.ReadAllText(filepath);
        var template = KernelFunctionYaml.ToPromptTemplateConfig(definition);
        var agent = new ChatCompletionAgent(template, new KernelPromptTemplateFactory())
                    {
                        Kernel = kernel
                    };
        var settings = new PromptExecutionSettings()
        {
            ServiceId = config["SemanticKernel:ServiceId"],
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };
        var arguments = new KernelArguments(settings)
        {
            { "visitor_details", messages.Last().Content },
        };
        var history = new ChatHistory();
        history.AddRange(messages);

        var result = agent.InvokeStreamingAsync(history, arguments);
        await foreach (var text in result)
        {
            yield return text.ToString();
        }
    }
}
