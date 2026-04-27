import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ResumeService } from '../../services/resume.service';
import { ResumeAnalysis } from '../../models/resume-analysis.model';

@Component({
  selector: 'app-resume-upload',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './resume-upload.component.html',
  styleUrls: ['./resume-upload.component.css']
})
export class ResumeUploadComponent {

  resumeText = '';
  selectedFile: File | null = null;       // holds PDF file if uploaded
  fileName = '';
  analysis: ResumeAnalysis | null = null;
  isLoading = false;
  errorMessage = '';

  constructor(private resumeService: ResumeService) {}

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.errorMessage = '';

    if (file.type === 'application/pdf') {
      // PDF — send directly to backend for server-side text extraction
      this.selectedFile = file;
      this.fileName = file.name;
      this.resumeText = '';           // clear text area when PDF is chosen
    } else if (file.type === 'text/plain') {
      // TXT — extract text in browser
      this.selectedFile = null;
      this.fileName = file.name;
      const reader = new FileReader();
      reader.onload = () => { this.resumeText = reader.result as string; };
      reader.readAsText(file);
    } else {
      this.errorMessage = 'Only PDF or .txt files are supported.';
    }
  }

  analyzeResume(): void {
    this.errorMessage = '';
    this.analysis = null;

    // PDF path
    if (this.selectedFile) {
      this.isLoading = true;
      this.resumeService.analyzePdf(this.selectedFile).subscribe({
        next: (result) => { this.analysis = result; this.isLoading = false; },
        error: (err: Error) => { this.errorMessage = err.message; this.isLoading = false; }
      });
      return;
    }

    // Text path
    if (!this.resumeText.trim()) {
      this.errorMessage = 'Please upload a file or paste resume text.';
      return;
    }
    this.isLoading = true;
    this.resumeService.analyzeResume(this.resumeText).subscribe({
      next: (result) => { this.analysis = result; this.isLoading = false; },
      error: (err: Error) => { this.errorMessage = err.message; this.isLoading = false; }
    });
  }

  clearAll(): void {
    this.resumeText = '';
    this.selectedFile = null;
    this.fileName = '';
    this.analysis = null;
    this.errorMessage = '';
  }
}
