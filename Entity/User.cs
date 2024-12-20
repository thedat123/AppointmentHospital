using Microsoft.AspNetCore.Identity;

namespace AppointmentHospital.Models
{
    public class User : IdentityUser<Guid>
    {
        public virtual Patient Patient { get; set; }
        public virtual Doctor Doctor { get; set; }
    }
}
