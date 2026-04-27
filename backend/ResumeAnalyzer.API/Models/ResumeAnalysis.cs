namespace ResumeAnalyzer.API.Models;

// Structured response returned to Angular after AI analysis
public class ResumeAnalysis
{
    public string Summary { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public List<string> SuggestedImprovements { get; set; } = new();
}
