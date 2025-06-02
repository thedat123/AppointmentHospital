using System;
using Newtonsoft.Json;

namespace AppointmentHospital.Models;

public class ChatRequest
{
    [JsonProperty("query")]
    public string Query { get; set; }

    [JsonProperty("include_context")]
    public bool IncludeContext { get; set; }

    [JsonProperty("max_results")]
    public int MaxResults { get; set; }

    [JsonProperty("patient_id")]
    public Guid PatientId { get; set; }

    [JsonProperty("session_id")]
    public Guid SessionId { get; set; }
}
