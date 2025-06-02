using System;
using System.Text;
using AppointmentHospital.Models;
using Newtonsoft.Json;

namespace AppointmentHospital.Services.Implement;

public class ChatbotService : IChatbotService
{
    private readonly HttpClient _httpClient;
    private readonly AppDbContext _context;
    
    public ChatbotService()
    {
        _httpClient = new HttpClient();
    }
    
    public async Task<ChatResponse> SendMessageAsync(ChatRequest chatRequest)
    {
        // First, ensure we have a valid session
        var session = new SessionRequest
        {
            SessionId = chatRequest.SessionId,
            PatientId = chatRequest.PatientId,
        };
        
        var sessionJson = JsonConvert.SerializeObject(session);
        var sessionContent = new StringContent(sessionJson, Encoding.UTF8, "application/json");
        
        Console.WriteLine("Creating/validating session...");
        var sessionResponse = await _httpClient.PostAsync("http://192.168.20.101:8000/session", sessionContent);
        
        if (!sessionResponse.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to create session: {sessionResponse.StatusCode}");
        }
        
        // Get the session ID from the response
        var sessionData = await sessionResponse.Content.ReadAsStringAsync();
        var sessionResult = JsonConvert.DeserializeObject<SessionResponse>(sessionData);
        
        // Use the session ID from the response for the chat request
        chatRequest.SessionId = sessionResult.SessionId;
        
        // Now send the chat request with the correct session ID
        var json = JsonConvert.SerializeObject(chatRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        Console.WriteLine($"Sending chat with session ID: {chatRequest.SessionId}");
        var response = await _httpClient.PostAsync("http://192.168.20.101:8000/chat", content);
        
        if (response.IsSuccessStatusCode)
        {
            var jsonData = await response.Content.ReadAsStringAsync();
            var chatResponse = JsonConvert.DeserializeObject<ChatResponse>(jsonData);
            return chatResponse;
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Chatbot API error: {response.StatusCode}, Details: {errorContent}");
        }
    }
}
