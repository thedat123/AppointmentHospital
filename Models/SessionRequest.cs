using System;
using Newtonsoft.Json;

namespace AppointmentHospital.Models;

public class SessionRequest
{
    [JsonProperty("patient_id")]
    public Guid PatientId { get; set; }

    [JsonProperty("session_id")]
    public Guid SessionId { get; set; }
}
