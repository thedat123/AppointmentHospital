using AppointmentHospital.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using AppointmentHospital.Helpers;
using AppointmentHospital.Repositories;
using AppointmentHospital.Repositories.Implement;
using AppointmentHospital.Services;
using AppointmentHospital.Services.Implement;
using AppointmentHospital.Areas.Admin.Repositories;
using AppointmentHospital.Areas.Admin.Repositories.Implement;
using AppointmentHospital.Areas.Admin.Services;
using AppointmentHospital.Areas.Admin.Services.Implement;
using Microsoft.AspNetCore.HttpOverrides;
using AppointmentHospital.Configuration.EmailConfiguaration;
using Hangfire;
using AppointmentHospital.Configuration.BaseUrl;
using DotNetEnv;

using System;
using System.Net;
using System.Net.Mail;

namespace AppointmentHospital
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            Env.Load();
            builder.Services.AddControllersWithViews();
            builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
                options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            });

            // Cấu hình Session với điều kiện môi trường
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                
                // Chỉ bật secure policy khi có HTTPS
                if (builder.Environment.IsProduction())
                {
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Thay đổi từ Always thành SameAsRequest
                    options.Cookie.SameSite = SameSiteMode.Lax;
                }
                else
                {
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                }
            });

            var configuration = builder.Configuration;
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<SeedData>();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<IAccountRepository, AccountRepository>();
            builder.Services.AddScoped<IManagingPatientRepository, ManagingPatientRepository>();
            builder.Services.AddScoped<IManagingPatientService, ManagingPatientService>();
            builder.Services.AddScoped<IManagingDoctorRepository, ManagingDoctorRepository>();
            builder.Services.AddScoped<IManagingDoctorService, ManagingDoctorService>();
            builder.Services.AddScoped<IStatisticService, StatisticService>();
            builder.Services.AddScoped<IStatisticRepository, StatisticRepository>();
            builder.Services.AddScoped<IAppointmentService, AppointmentService>();
            builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            builder.Services.AddScoped<ISpecialityService, SpecialityService>();
            builder.Services.AddScoped<ISpecialityRepository, SpecialityRepository>();
     
            builder.Services.AddScoped<IPatientService, PatientService>();
            builder.Services.AddScoped<IPatientRepository, PatientRepository>();
            builder.Services.Configure<EmailConfiguration>(configuration.GetSection("SMTP"));
            builder.Services.Configure<BaseUrl>(configuration.GetSection("BaseUrl"));
            builder.Services.AddTransient<IEmailService, EmailService>();
            

            builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
            builder.Services.AddScoped<IDoctorService, DoctorService>();
            builder.Services.AddScoped<ITimeSlotRepository, TimeSlotRepository>();
            builder.Services.AddScoped<ITimeSlotService, TimeSlotService>();

            builder.Services.AddScoped<IAppointmentDateRepository, AppointmentDateRepository>();
            builder.Services.AddScoped<IAppointmentDateService, AppointmentDateService>();
            

            builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
            builder.Services.AddScoped<IDoctorService, DoctorService>();

            builder.Services.AddScoped<IDiseasePredictionService, DiseasePredictionService>();
            builder.Services.AddScoped<ICronTimeSlotService, CronTimeSlotService>();
            
            builder.Services.AddScoped<ISpecialitiesRepository, SpecialitiesRepository>();
            builder.Services.AddScoped<ISpecialitiesService, SpecialitiesService>();
            builder.Services.AddScoped<IChatbotService, ChatbotService>();

            // Cấu hình Google Authentication với điều kiện môi trường
            builder.Services.AddAuthentication().AddGoogle(option =>
            {
                var clientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENTID");
                var clientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENTSECRET");
                
                option.CallbackPath = "/dang-nhap-bang-google";
                option.ClientId = clientId;
                option.ClientSecret = clientSecret;
                
                // Cấu hình cookie cho production
                if (builder.Environment.IsProduction())
                {
                    option.CorrelationCookie.SameSite = SameSiteMode.Lax;
                    option.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Thay đổi từ Always
                }
            });

            builder.Services.AddScoped<IAppointmentDateRepository, AppointmentDateRepository>();
            builder.Services.AddScoped<IAppointmentDateService, AppointmentDateService>();
            
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("AppointmentHospitalDB")));

            builder.Services.AddIdentity<User, IdentityRole<Guid>>()
                            .AddEntityFrameworkStores<AppDbContext>()
                            .AddDefaultTokenProviders();

            builder.Services.Configure<IdentityOptions>(options => {
                // Thiết lập về Password
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false; 
                options.Password.RequireNonAlphanumeric = false; 
                options.Password.RequireUppercase = false; 
                options.Password.RequiredLength = 3;
                options.Password.RequiredUniqueChars = 1; 

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5; 
                options.Lockout.AllowedForNewUsers = true;

                // Cấu hình về User.
                options.User.AllowedUserNameCharacters = 
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;  

                // Cấu hình đăng nhập.
                options.SignIn.RequireConfirmedEmail = true;            
                options.SignIn.RequireConfirmedPhoneNumber = false;    
            });

            // Cấu hình ApplicationCookie với điều kiện môi trường
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(45);
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDeny";
                
                // Cấu hình cho production
                if (builder.Environment.IsProduction())
                {
                    options.Cookie.Domain = "medicalcare.io.vn";
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Thay đổi từ Always
                }
            });

            // Cấu hình Data Protection và Antiforgery
            if (builder.Environment.IsProduction())
            {
                // Data Protection
                builder.Services.AddDataProtection()
                    .SetApplicationName("AppointmentHospital");
                
                // Antiforgery - SỬA LỖI CHÍNH TẠI ĐÂY
                builder.Services.AddAntiforgery(options =>
                {
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Thay đổi từ Always thành SameAsRequest
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.HttpOnly = true;
                });
            }
            else
            {
                // Cấu hình cho development
                builder.Services.AddAntiforgery(options =>
                {
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.HttpOnly = true;
                });
            }

            builder.Services.AddHangfire(config =>
            {
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(configuration.GetConnectionString("AppointmentHospitalDB"));
            });
            builder.Services.AddHangfireServer();
            builder.Services.AddSignalR().AddNewtonsoftJsonProtocol(options =>
            {
                options.PayloadSerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });

            // BUILD APP SAU KHI ĐÃ CẤU HÌNH TẤT CẢ SERVICES
            var app = builder.Build();
            
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // Cấu hình Forwarded Headers cho reverse proxy
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHangfireDashboard();

            using (var scope = app.Services.CreateScope())
            {
                var cronTimeSlotService = scope.ServiceProvider.GetRequiredService<ICronTimeSlotService>();
                await cronTimeSlotService.DeleteOldTimeSlotAsync();
            }

            app.MapHub<ScheduleHub>("/scheduleHub");
            app.MapAreaControllerRoute(
            name: "admin",
            areaName: "Admin",
            pattern: "{controller}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Patient}/{action=Index}/{id?}");
                
            app.MapHangfireDashboard("/hangfire");

            app.Run();
        }
    }
}