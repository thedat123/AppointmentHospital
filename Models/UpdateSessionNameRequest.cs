using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppointmentHospital.Models
{
    public class UpdateSessionNameRequest
    {
        public string SessionId { get; set; }
        public string SessionName { get; set; }
    }
}