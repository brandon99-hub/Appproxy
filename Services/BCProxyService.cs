using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using BIProxy.Models;

namespace BIProxy.Services;

public class BCProxyService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BCProxyService> _logger;

    public BCProxyService(
        HttpClient httpClient,
        ILogger<BCProxyService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<string> PostAdmissionAsync(BCAdmission admission)
    {
        var json = JsonSerializer.Serialize(admission, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogInformation("Posting to Admissions");

        var response = await _httpClient.PostAsync("Admissions", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Error posting admission: {StatusCode} {Content}", response.StatusCode, responseContent);
            throw new Exception($"Failed to post admission: {response.StatusCode} - {responseContent}");
        }

        var jsonResponse = JsonNode.Parse(responseContent);
        var admissionNo = jsonResponse?["Admission_No"]?.GetValue<string>();
        
        return admissionNo ?? throw new Exception("Failed to retrieve Admission_No from response");
    }

    public async Task PostAppSchoolAsync(BCAppSchool school)
    {
        var json = JsonSerializer.Serialize(school, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("AppSchools", content);
        if (!response.IsSuccessStatusCode)
        {
            var res = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to post AppSchool: {response.StatusCode} - {res}");
        }
    }

    public async Task PostAppRelativeAsync(BCAppRelative relative)
    {
        var json = JsonSerializer.Serialize(relative, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("AppRelatives", content);
        if (!response.IsSuccessStatusCode)
        {
            var res = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to post AppRelative: {response.StatusCode} - {res}");
        }
    }

    public async Task PostAppRelationAsync(BCAppRelation relation)
    {
        var json = JsonSerializer.Serialize(relation, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("AppRelations", content);
        if (!response.IsSuccessStatusCode)
        {
            var res = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to post AppRelation: {response.StatusCode} - {res}");
        }
    }
    public async Task PostAdmissionParentAsync(BCAdmissionParent parent)
    {
        var json = JsonSerializer.Serialize(parent, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("Admissionparents", content);
        if (!response.IsSuccessStatusCode)
        {
            var res = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to post AdmissionParent: {response.StatusCode} - {res}");
        }
    }
}
