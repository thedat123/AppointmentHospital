using System;
using System.Text;
using AppointmentHospital.Models;
using Newtonsoft.Json;

namespace AppointmentHospital.Services.Implement;

public class ChatbotService : IChatbotService
{
    private readonly HttpClient _httpClient;

    public ChatbotService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<ChatResponse> SendMessageAsync(ChatRequest chatRequest)
    {
        var json = JsonConvert.SerializeObject(chatRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("http://192.168.1.106:8000/chat", content);

        if (response.IsSuccessStatusCode)
        {
            var jsonData = await response.Content.ReadAsStringAsync();
            var chatResponse = JsonConvert.DeserializeObject<ChatResponse>(jsonData);
            return chatResponse;
        }
        else
        {
            throw new Exception($"Chatbot API error: {response.StatusCode}");
        }
    }

}
