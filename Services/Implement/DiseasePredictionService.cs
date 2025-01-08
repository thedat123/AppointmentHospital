using System;
using System.Text;
using Newtonsoft.Json;

namespace AppointmentHospital.Services.Implement;

public class DiseasePredictionService : IDiseasePredictionService
{
    private readonly HttpClient _httpClient;
    public DiseasePredictionService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<dynamic> PredictDiseaseAsync(string[] symptoms)
    {
        var url = "http://127.0.0.1:5005/predict";
        var data = new
        {
            symptoms = symptoms
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(data),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync(url, content);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent != null ? JsonConvert.DeserializeObject<dynamic>(responseContent) : null;
        }

        return null;
    }

}