using System;
using System.Collections.Generic;
using System.Linq;
using AppointmentHospital.Entity;
using AppointmentHospital.EnumStatus;
using AppointmentHospital.Helpers;
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

        public List<Appointment> GetAppointmentsByPatientId(Guid patientId, AppointmentStatus status){
            List<Appointment> appointments = _context.Appointments.Where(a => a.PatientId == patientId && a.Status == status).ToList();
            return appointments;
        }

        public async Task<Pagination<Appointment>> GetAppointmentsByDoctorId(Guid doctorId, int page){
            var appointments = _context.Appointments.Where(a => a.DoctorId == doctorId);
            var paginatedAppointments = await Pagination<Appointment>.PaginatedList(appointments, page);
            return paginatedAppointments;
        }

        public async Task<Pagination<Appointment>> GetAppointmentsByDoctorId(Guid doctorId, AppointmentStatus status, int page){
            var appointments = _context.Appointments.Where(a => a.DoctorId == doctorId && a.Status == status);
            var paginatedAppointments = await Pagination<Appointment>.PaginatedList(appointments, page);
            return paginatedAppointments;
        }

        public Appointment GetAppointmentsById(Guid appointmentId){
            return _context.Appointments.FirstOrDefault(a => a.AppointmentId == appointmentId);
        }

        public void UpdateStatusAppointment(Guid appointmentId, AppointmentStatus status){
            var appointment = _context.Appointments.FirstOrDefault(a => a.AppointmentId == appointmentId);
            if (appointment == null)
            {
                throw new InvalidOperationException("No appointment found for the given id.");
            }

            appointment.Status = status;
            _context.SaveChanges();
        }

        public List<Appointment> GetAllAppointments()
        {
            return _context.Appointments.ToList();
        }

        public Appointment GetAppointmentsByDoctorIdAndStartTime(Guid doctorId, DateTime startTime)
        {
            return _context.Appointments
                .Where(a => a.DoctorId.Equals(doctorId) &&
                            a.AppointmentTime.Year == startTime.Year &&
                            a.AppointmentTime.Month == startTime.Month &&
                            a.AppointmentTime.Day == startTime.Day &&
                            a.AppointmentTime.Hour == startTime.Hour &&
                            a.AppointmentTime.Minute == startTime.Minute)
                .FirstOrDefault();
        }

        public DiagnosisHistory GetDiagnosisHistoriesByAppointmentID(Guid appointmentId)
        {
            return _context.DiagnosisHistory.Where(d => d.AppointmentId == appointmentId).FirstOrDefault();
        }

        public List<Appointment> GetAppointmentsByDoctorIdAndDate(Guid doctorId, DateTime date){
            return _context.Appointments.Where(a => a.DoctorId == doctorId && a.AppointmentTime.Date == date.Date).ToList();    
        }
    }
}