using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ResumeAnalyzer.API.Models;

namespace ResumeAnalyzer.API.Services;

public class AIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<AIService> _logger;

    public AIService(HttpClient httpClient, IConfiguration config, ILogger<AIService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    public async Task<ResumeAnalysis> AnalyzeResumeAsync(string resumeText)
    {
        // Try nested config first (local), then plain env var (Render/cloud)
        var apiKey = _config["OpenAI:ApiKey"]
            ?? Environment.GetEnvironmentVariable("GROQ_API_KEY")
            ?? throw new InvalidOperationException("API key is not configured.");

        var model    = _config["OpenAI:Model"]    ?? "gemini-2.0-flash";
        var endpoint = _config["OpenAI:Endpoint"] ?? "https://generativelanguage.googleapis.com/v1beta/openai/chat/completions";

        var systemPrompt =
            "You are an expert career coach and resume reviewer. " +
            "Analyze the provided resume and return ONLY a valid JSON object " +
            "(no markdown, no code fences, no explanation) with exactly these keys: " +
            "\"summary\" (string), " +
            "\"strengths\" (array of strings), " +
            "\"weaknesses\" (array of strings), " +
            "\"suggestedImprovements\" (array of strings).";

        var requestBody = new
        {
            model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user",   content = $"Here is the resume to analyze:\n\n{resumeText}" }
            },
            temperature = 0.4
        };

        var json    = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Both Gemini OpenAI-compatible endpoint and OpenAI use Authorization: Bearer
        _httpClient.DefaultRequestHeaders.Remove("Authorization");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        var response = await _httpClient.PostAsync(endpoint, content);

        // Log and surface the raw error body so we can see exactly what went wrong
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogError("AI API error {Status}: {Body}", (int)response.StatusCode, errorBody);

            var msg = (int)response.StatusCode switch
            {
                401 => "Invalid API key.",
                429 => "Rate limit exceeded. Try again in a moment.",
                _   => $"AI API returned {(int)response.StatusCode}: {errorBody}"
            };
            throw new HttpRequestException(msg);
        }

        var responseJson = await response.Content.ReadAsStringAsync();
        _logger.LogDebug("AI raw response: {Response}", responseJson);

        // Extract the text content from the chat completions wrapper
        using var doc = JsonDocument.Parse(responseJson);
        var aiText = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "{}";

        // Strip markdown code fences if the model wrapped the JSON (e.g. ```json ... ```)
        aiText = Regex.Replace(aiText, @"^```(?:json)?\s*|\s*```$", "", RegexOptions.Multiline).Trim();

        var options  = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var analysis = JsonSerializer.Deserialize<ResumeAnalysis>(aiText, options)
            ?? throw new InvalidOperationException("Failed to deserialize AI response.");

        return analysis;
    }
}
