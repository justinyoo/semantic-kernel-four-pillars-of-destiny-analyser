using FourPillarsAnalyser.WebApp.Models;

namespace FourPillarsAnalyser.WebApp.ApiClients;

public interface IChatClient
{
    IAsyncEnumerable<string> CompleteChatStreamingAsync(IEnumerable<ChatMessage> messages);
}

public class ChatClient(HttpClient http) : IChatClient
{
    private const string REQUEST_URI = "api/chat/complete";

    public async IAsyncEnumerable<string> CompleteChatStreamingAsync(IEnumerable<ChatMessage> messages)
    {
        var content = messages.Select(p => new PromptRequest(p.Role, p.Content));
        var response = await http.PostAsJsonAsync<IEnumerable<PromptRequest>>($"{REQUEST_URI}", content);

        response.EnsureSuccessStatusCode();

        var result = response.Content.ReadFromJsonAsAsyncEnumerable<PromptResponse>();
        await foreach (var message in result)
        {
            yield return message!.Content;
        }
    }
}
