namespace FourPillarsAnalyser.WebApp.Models;

public class ChatMessage(string role, string content)
{
    public string Role { get; } = role;
    public string Content { get; set; } = content;
}