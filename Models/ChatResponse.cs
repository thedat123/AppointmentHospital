using System;
using Newtonsoft.Json;

namespace AppointmentHospital.Models;

public class ChatResponse
{
    [JsonProperty("response")]
    public string Response { get; set; }
    
    [JsonProperty("session_id")]
    public Guid SessionId { get; set; }
    
    [JsonProperty("processing_time")]
    public double ProcessingTime { get; set; }
    
    [JsonProperty("metrics")]
    public Dictionary<string, object> Metrics { get; set; }
    
    [JsonProperty("context")]
    public List<object> Context { get; set; }
    
    [JsonProperty("evaluation")]
    public object Evaluation { get; set; }
}
