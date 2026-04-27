// Mirrors the ResumeAnalysis C# model returned by the backend
export interface ResumeAnalysis {
  summary: string;
  strengths: string[];
  weaknesses: string[];
  suggestedImprovements: string[];
}
