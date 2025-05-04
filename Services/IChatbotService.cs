using System;
using AppointmentHospital.Models;

namespace AppointmentHospital.Services;

public interface IChatbotService
{
    public Task<ChatResponse> SendMessageAsync(ChatRequest chatRequest);
}
