using System;
using AppointmentHospital.Entity;
using AppointmentHospital.Models;
using AppointmentHospital.Repositories;

namespace AppointmentHospital.Services.Implement;

public class AppointmentDateService : IAppointmentDateService
{
    private readonly IAppointmentDateRepository doctorRepository;

    public AppointmentDateService(IAppointmentDateRepository _doctorRepository){
        doctorRepository = _doctorRepository;
    }

    public List<Appointment> GetAppointmentByDoctorId(Guid DoctorId){
        return doctorRepository.GetAppointmentByDoctorId(DoctorId);
    }

}
