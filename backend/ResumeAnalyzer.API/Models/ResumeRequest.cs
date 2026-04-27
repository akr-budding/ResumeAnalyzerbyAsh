namespace ResumeAnalyzer.API.Models;

public class ResumeRequest
{
    // Raw resume text extracted from PDF or pasted directly
    public string ResumeText { get; set; } = string.Empty;
}
