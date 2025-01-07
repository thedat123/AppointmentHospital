using System;

namespace AppointmentHospital.Services;

public interface IDiseasePredictionService
{
    public Task<dynamic> PredictDiseaseAsync(string[] symptoms);
}
