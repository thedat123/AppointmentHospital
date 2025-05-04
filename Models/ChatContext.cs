using System;
using Newtonsoft.Json;

namespace AppointmentHospital.Models;

public class ChatContext
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("text")]
    public string Text { get; set; }

    [JsonProperty("disease_name")]
    public string DiseaseName { get; set; }

    [JsonProperty("source")]
    public string Source { get; set; }

    [JsonProperty("score")]
    public float Score { get; set; }
}
