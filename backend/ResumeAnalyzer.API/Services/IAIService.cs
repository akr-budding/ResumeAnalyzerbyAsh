using ResumeAnalyzer.API.Models;

namespace ResumeAnalyzer.API.Services;

// Interface allows easy swapping of AI providers (OpenAI → Azure OpenAI, etc.)
public interface IAIService
{
    Task<ResumeAnalysis> AnalyzeResumeAsync(string resumeText);
}
