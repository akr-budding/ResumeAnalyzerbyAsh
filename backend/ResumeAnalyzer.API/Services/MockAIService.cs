using ResumeAnalyzer.API.Models;

namespace ResumeAnalyzer.API.Services;

// Used when OpenAI:UseMock = true in appsettings — no API key needed
public class MockAIService : IAIService
{
    public Task<ResumeAnalysis> AnalyzeResumeAsync(string resumeText)
    {
        var result = new ResumeAnalysis
        {
            Summary = "This is a mock analysis. The candidate appears to be an experienced software developer " +
                      "with a strong background in full-stack development and cloud technologies.",
            Strengths = new List<string>
            {
                "Strong technical skill set across multiple languages and frameworks",
                "Demonstrated experience with cloud platforms (Azure / AWS)",
                "Good mix of frontend and backend expertise",
                "Clear progression of roles showing career growth"
            },
            Weaknesses = new List<string>
            {
                "Resume lacks quantified achievements (e.g., improved performance by X%)",
                "No mention of soft skills or team leadership",
                "Education section is brief and could be expanded"
            },
            SuggestedImprovements = new List<string>
            {
                "Add measurable impact to each bullet point (e.g., 'reduced load time by 40%')",
                "Include a professional summary at the top",
                "Add links to GitHub profile or portfolio projects",
                "List certifications (Azure, AWS, etc.) in a dedicated section"
            }
        };

        return Task.FromResult(result);
    }
}
