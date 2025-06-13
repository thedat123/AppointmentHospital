using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<Pagination<Appointment>> GetAppointmentsByPatientId(Guid patientId, int page){
            var appointments = _context.Appointments.Where(a => a.PatientId == patientId);
            var paginatedAppointments = await Pagination<Appointment>.PaginatedList(appointments, page, 3);
            return paginatedAppointments;
        }

        public async Task<Pagination<Appointment>> GetAppointmentsByPatientId(Guid patientId, AppointmentStatus status, int page){
            var appointments = _context.Appointments.Where(a => a.PatientId == patientId && a.Status == status);
            var paginatedAppointments = await Pagination<Appointment>.PaginatedList(appointments, page, 3);
            return paginatedAppointments;
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

        public int CountAppointmentDoctorIdStatus(Guid doctorId, AppointmentStatus status){
            return _context.Appointments.Where(a => a.DoctorId == doctorId && a.Status == status).Count();
        }

        public async Task<Pagination<Appointment>> GetAllPendingAppointments(int page, int? status = null, string searchTerm = null, DateTime? appointmentDate = null)
        {
            // Start with base query including all necessary relationships
            var query = _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.Collaborator)
                .Include(a => a.Acquaintance)
                .AsQueryable();

            // Filter by status
            if (status.HasValue)
            {
                var appointmentStatus = (AppointmentStatus)status.Value;
                query = query.Where(a => a.Status == appointmentStatus);
            }
            else
            {
                // Default: include Pending, Confirmed, Canceled statuses
                query = query.Where(a => a.Status == AppointmentStatus.Pending || 
                                        a.Status == AppointmentStatus.Confirmed || 
                                        a.Status == AppointmentStatus.Canceled);
            }

            // Filter by search term (name or appointment code)
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(a => 
                    a.Patient.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (a.Acquaintance != null && a.Acquaintance.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    a.AppointmentId.ToString().Contains(searchTerm));
            }

            // Filter by appointment date
            if (appointmentDate.HasValue)
            {
                var startDate = appointmentDate.Value.Date;
                var endDate = startDate.AddDays(1);
                query = query.Where(a => a.AppointmentTime >= startDate && a.AppointmentTime < endDate);
            }

            // Order by appointment time (newest first)
            query = query.OrderByDescending(a => a.AppointmentTime);

            // Apply pagination
            var paginatedAppointments = await Pagination<Appointment>.PaginatedList(query, page);
            
            return paginatedAppointments;
        }
    }
}