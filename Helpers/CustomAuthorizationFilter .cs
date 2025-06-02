using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AppointmentHospital.Helpers
{
    public class CustomAuthorizationFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Kiểm tra xem action có [AllowAnonymous] không
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata
                .Any(em => em.GetType() == typeof(AllowAnonymousAttribute));

            if (!allowAnonymous && !context.HttpContext.User.Identity.IsAuthenticated)
            {
                // Kiểm tra xem request có phải là AJAX không
                var request = context.HttpContext.Request;
                bool isAjaxRequest = request.Headers.ContainsKey("X-Requested-With") &&
                                    request.Headers["X-Requested-With"] == "XMLHttpRequest";
                                 
                // Hoặc kiểm tra Accept header
                bool acceptsJson = request.Headers.ContainsKey("Accept") &&
                                  request.Headers["Accept"].ToString().Contains("application/json");

                // Luôn trả về JSON response để xử lý bằng JavaScript
                context.Result = new JsonResult(new 
                {
                    requireLogin = true,
                    message = "Vui lòng đăng nhập để sử dụng tính năng này",
                    loginUrl = "/Account/Login"
                })
                {
                    StatusCode = 401 // Unauthorized
                };
            }
        }
    }
}