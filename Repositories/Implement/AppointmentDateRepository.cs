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

        public void AddAppointment(Appointment appointment)
        {
            _context.Appointments.Add(appointment);
            _context.SaveChanges();
        }

        public List<Appointment> GetAppointmentsByPatientId(Guid patientId){
            List<Appointment> appointments = _context.Appointments.Where(a => a.PatientId == patientId).ToList();
            return appointments;
        }

        public List<Appointment> GetAppointmentsByDoctorId(Guid doctorId){
            List<Appointment> appointments = _context.Appointments.Where(a => a.DoctorId == doctorId).ToList();
            return appointments;
        }
    }
}