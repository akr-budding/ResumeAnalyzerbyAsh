using Microsoft.AspNetCore.Mvc;
using ResumeAnalyzer.API.Models;
using ResumeAnalyzer.API.Services;
using UglyToad.PdfPig;

namespace ResumeAnalyzer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResumeController : ControllerBase
{
    private readonly IAIService _aiService;
    private readonly ILogger<ResumeController> _logger;

    public ResumeController(IAIService aiService, ILogger<ResumeController> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    // POST api/resume/upload
    // Accepts a PDF file, extracts text, then runs AI analysis
    [HttpPost("upload")]
    [RequestSizeLimit(5 * 1024 * 1024)] // 5 MB max
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file uploaded." });

        if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Only PDF files are accepted." });

        // Extract all text from every page using PdfPig
        var sb = new System.Text.StringBuilder();
        using (var stream = file.OpenReadStream())
        using (var pdf = PdfDocument.Open(stream))
        {
            foreach (var page in pdf.GetPages())
                sb.AppendLine(page.Text);
        }

        var resumeText = sb.ToString().Trim();
        if (string.IsNullOrWhiteSpace(resumeText))
            return BadRequest(new { error = "Could not extract text from the PDF. Try a text-based PDF." });

        try
        {
            _logger.LogInformation("PDF upload: extracted {Length} chars, analyzing...", resumeText.Length);
            var analysis = await _aiService.AnalyzeResumeAsync(resumeText);
            return Ok(analysis);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "AI API call failed.");
            return StatusCode(502, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during PDF analysis.");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    // POST api/resume/analyze
    // Receives resume text from Angular and returns AI analysis
    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze([FromBody] ResumeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ResumeText))
            return BadRequest(new { error = "Resume text cannot be empty." });

        // Guard against extremely large payloads (basic protection)
        if (request.ResumeText.Length > 15_000)
            return BadRequest(new { error = "Resume text is too long (max 15,000 characters)." });

        try
        {
            _logger.LogInformation("Analyzing resume, length={Length}", request.ResumeText.Length);
            var analysis = await _aiService.AnalyzeResumeAsync(request.ResumeText);
            return Ok(analysis);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "AI API call failed.");
            return StatusCode(502, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during resume analysis.");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }
}
