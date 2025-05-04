using System;
using Newtonsoft.Json;

namespace AppointmentHospital.Models;

public class ChatResponse
{
    [JsonProperty("response")]
    public string Response { get; set; }

    [JsonProperty("processing_time")]
    public double ProcessingTime { get; set; }

    [JsonProperty("context")]
    public List<ChatContext> Context { get; set; }

    [JsonProperty("metrics")]
    public Dictionary<string, object> Metrics { get; set; }
}
