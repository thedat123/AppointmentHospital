using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppointmentHospital.Models
{
    public class SaveMessageRequest
    {
        public string SessionId { get; set; }
        public string MessageText { get; set; }
        public bool IsFromPatient { get; set; }
    }
}