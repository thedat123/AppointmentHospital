using System;
using Newtonsoft.Json;

namespace AppointmentHospital.Models;

public class SessionResponse
{
    [JsonProperty("is_new")]
    public bool IsNew { get; set; }
    
    [JsonProperty("session_id")]
    public Guid SessionId { get; set; }
    
}
