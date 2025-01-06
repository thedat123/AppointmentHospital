using AppointmentHospital.Entity;
using AppointmentHospital.EnumStatus;
using AppointmentHospital.Models;
using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AppointmentHospital.Helpers
{
    public class SeedData
    {
        private readonly AppDbContext _context;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly UserManager<User> _userManager;
        public SeedData(AppDbContext context, RoleManager<IdentityRole<Guid>> roleManager, UserManager<User> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public async Task CreateAppointment (Guid patientId, Guid doctorId, List<DateTime> appointmentTime)
        {
            var appointment = new Appointment()
            {
                PatientId = patientId,
                DoctorId = doctorId,
                AppointmentTime = new Faker().PickRandom(appointmentTime),
                Notes = "Seed data appointment",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CancellationReason = null,
                Status = AppointmentStatus.Completed,
                Symptoms = "Headache"
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
        }

        public async Task CreateDoctorTimeSlot(Doctor doctor)
        {
            var fakeTimeSlot = new Faker<TimeSlot>()
                    .RuleFor(ts => ts.DoctorId, f => doctor.DoctorId)
                    .RuleFor(ts => ts.StartTime, f => new DateTime(2024, f.Random.Int(6,12), f.Random.Int(1, 30), 9, 0, 0))
                    .RuleFor(ts => ts.EndTime, (f, ts) => ts.StartTime.AddHours(1));
            
               var timeSlot =  fakeTimeSlot.Generate(new Faker().Random.Int(20,30));
               await _context.TimeSlots.AddRangeAsync(timeSlot);
               await _context.SaveChangesAsync();
        }
       
        public async Task InitialData()
        {
            if (!(await _roleManager.RoleExistsAsync("Admin")))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>("Admin"));
            }
            if (!(await _roleManager.RoleExistsAsync("Patient")))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>("Patient"));
            }
            if (!(await _roleManager.RoleExistsAsync("Doctor")))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>("Doctor"));
            }

            if (!(await _context.Users.AnyAsync()))
            {
                var admin = new User
                {
                    Email = "admin@gmail.com",
                    UserName = "admin@gmail.com",
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(admin, "Admin123#");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(admin, "Admin");
                }
                var user = new User
                {
                    Email = "patient1@gmail.com",
                    UserName = "patient1@gmail.com",
                    EmailConfirmed = true
                };
                var createPatientResult = await _userManager.CreateAsync(user, "Patient123#");
                if (createPatientResult.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Patient");
                    var patient = new Patient
                    {
                        PatientId = user.Id,
                        FullName = "John Bracker",
                        Address = "Viet nam",
                        DateOfBirth = new DateTime(2002, 11, 11),
                        User = user
                    };
                    _context.Patients.Add(patient);
                    await _context.SaveChangesAsync();
                }
                var user2 = new User
                {
                    Email = "doctor@gmail.com",
                    UserName = "doctor@gmail.com",
                    EmailConfirmed = true
                };


                var createDoctorResult = await _userManager.CreateAsync(user2, "Doctor123#");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user2, "Doctor");
             
                    var doctor = new Doctor
                    {
                        DoctorId = user2.Id,
                        FullName = "John Bracker",
                        Specializaiton = Specialization.NgoaiKhoa,
                        ExperienceYear = 3,
                        User = user2,
                        Degree = "PhD",
                        ImagePath = "~/images/doctor",
                        Description = "Tôi là bác sĩ John Bracker",
                        Gender = "Female"
                    };
                    _context.Doctors.Add(doctor);
                    await _context.SaveChangesAsync();
                }

                var fakerUser = new Faker<User>()
                .RuleFor(u => u.UserName, f => f.Internet.Email())
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber())
                .RuleFor(u => u.EmailConfirmed, f => true);

                var users = fakerUser.Generate(20);
                var patientList = new List<Patient>();
                var doctorList = new List<Doctor>();

                foreach (var patientFake in users.Take(12))
                {
                    var createResult = await _userManager.CreateAsync(patientFake, "Patient123#");
                    if (createResult.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(patientFake, "Patient");

                        var fakerPatient = new Faker<Patient>()
                        .RuleFor(p => p.Address, f => f.Address.FullAddress())
                        .RuleFor(p => p.DateOfBirth, f => f.Date.Past(50, DateTime.Now.AddYears(-18)))
                        .RuleFor(p => p.FullName, f => f.Name.FullName());
                        var patient = fakerPatient.Generate();
                        patient.PatientId = patientFake.Id;
                        await _context.Patients.AddAsync(patient);
                        await _context.SaveChangesAsync();
                        patientList.Add(patient);
                    }
                }
                foreach (var doctorFake in users.Skip(12))
                {
                    var createResult = await _userManager.CreateAsync(doctorFake, "Doctor123#");
                    if (createResult.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(doctorFake, "Doctor");
                        var fakeDoctor = new Faker<Doctor>()
                       .RuleFor(d => d.FullName, f => f.Name.FullName())
                       .RuleFor(d => d.Specializaiton, f => f.PickRandom<Specialization>())
                       .RuleFor(d => d.ExperienceYear, f => f.Random.Int(1, 5))
                       .RuleFor(d => d.Gender, f => f.PickRandom(new List<string> { "Male", "Female"}))
                       .RuleFor(d => d.Description, f => f.Lorem.Sentence(10))
                       .RuleFor(d => d.Degree, f => f.PickRandom(new List<string>
                        {
                            "MD",
                            "DO",
                            "MBBS",
                            "PhD"
                        }));
                        var doctorFaker = fakeDoctor.Generate();
                        doctorFaker.DoctorId = doctorFake.Id;
                        doctorFaker.ImagePath = "~/images/doctor";
                        _context.Doctors.Add(doctorFaker);
                        await _context.SaveChangesAsync();
                        await CreateDoctorTimeSlot(doctorFaker);
                        doctorList.Add(doctorFaker);
                    }
                }
                var faker = new Faker();
                var ramdomInt = faker.Random.Int(10,15);

                foreach ( var patient in patientList )
                {
                    for (int i = 0; i< ramdomInt; i++)
                    {
                        var doctorId = faker.PickRandom(doctorList).DoctorId;
                        var appointmentTime = await _context.TimeSlots.Where(ts => ts.DoctorId == doctorId)
                                                                      .Select(ts => ts.StartTime)
                                                                      .ToListAsync();
                        await CreateAppointment(patient.PatientId, doctorId, appointmentTime);
                    }
                }


            }
        }
    }
}
