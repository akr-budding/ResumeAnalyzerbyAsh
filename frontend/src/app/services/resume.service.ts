import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ResumeAnalysis } from '../models/resume-analysis.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ResumeService {

  private readonly apiUrl = `${environment.apiBaseUrl}/api/resume/analyze`;

  constructor(private http: HttpClient) {}

  // Send resume text to .NET API and receive structured analysis
  analyzeResume(resumeText: string): Observable<ResumeAnalysis> {
    return this.http
      .post<ResumeAnalysis>(this.apiUrl, { resumeText })
      .pipe(catchError(this.handleError));
  }

  // Send PDF file to .NET API — backend extracts text and runs AI analysis
  analyzePdf(file: File): Observable<ResumeAnalysis> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http
      .post<ResumeAnalysis>(`${environment.apiBaseUrl}/api/resume/upload`, formData)
      .pipe(catchError(this.handleError));
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    const message =
      error.error?.error ??
      (error.status === 0
        ? 'Cannot reach the server. Is the backend running?'
        : `Server error ${error.status}`);
    return throwError(() => new Error(message));
  }
}
