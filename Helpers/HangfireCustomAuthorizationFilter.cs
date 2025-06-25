using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Dashboard;

namespace AppointmentHospital.Helpers
{
    public class HangfireCustomAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly IHttpContextAccessor _contextAccessor;
        public bool Authorize(DashboardContext context)
        {
            var httpContext = _contextAccessor.HttpContext;
            // Example: Allow only authenticated admins
            return httpContext.User.Identity.IsAuthenticated && httpContext.User.IsInRole("Admin");
        }
    }
}