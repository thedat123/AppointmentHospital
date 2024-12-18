using System;
using System.Collections.Generic;
using System.Linq;
using AppointmentHospital.Entity;
using AppointmentHospital.Models;
using Microsoft.EntityFrameworkCore;

namespace AppointmentHospital.Repositories.Implement
{
    public class AppointmentDateRepository : IAppointmentDateRepository
    {
        private readonly AppDbContext _context;

        public AppointmentDateRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Appointment> GetAppointmentByDoctorId(Guid doctorId)
        {
            var appointments = _context.Appointments
                .Where(d => d.DoctorId == doctorId)
                .ToList();
            
            if (!appointments.Any())
                throw new InvalidOperationException("No appointments found for the given doctor.");

            return appointments;
        }
    }
}