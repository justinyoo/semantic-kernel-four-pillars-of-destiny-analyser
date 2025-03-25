using FourPillarsAnalyser.ApiApp.Models;
using FourPillarsAnalyser.ApiApp.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel.ChatCompletion;

using ChatMessageContent = Microsoft.SemanticKernel.ChatMessageContent;

namespace FourPillarsAnalyser.ApiApp.Endpoints;

public static class ChatCompletionEndpoint
{
    public static IEndpointRouteBuilder MapChatCompletionEndpoint(this IEndpointRouteBuilder routeBuilder)
    {
        var api = routeBuilder.MapGroup("api/chat");

        api.MapPost("complete", PostChatCompletionAsync)
           .Accepts<PromptRequest>(contentType: "application/json")
           .Produces<IEnumerable<PromptResponse>>(statusCode: StatusCodes.Status200OK, contentType: "application/json")
           .WithTags("chat")
           .WithName("ChatCompletion")
           .WithOpenApi();

        return routeBuilder;
    }

    public static async IAsyncEnumerable<PromptResponse> PostChatCompletionAsync([FromBody] IEnumerable<PromptRequest> req, IKernelService service)
    {
        var messages = new List<ChatMessageContent>();
        foreach (var msg in req)
        {
            ChatMessageContent message = msg.Role switch
            {
                "User" => new ChatMessageContent(AuthorRole.User, msg.Content),
                "Assistant" => new ChatMessageContent(AuthorRole.Assistant, msg.Content),
                "System" => new ChatMessageContent(AuthorRole.System, msg.Content),
                _ => throw new ArgumentException($"Invalid role: {msg.Role}")
            };
            messages.Add(message);
        }

        var result = service.CompleteChatStreamingAsync(messages);
        await foreach (var text in result)
        {
            yield return new PromptResponse(text);
        }
    }
}
