import { Component } from '@angular/core';
import { ResumeUploadComponent } from './components/resume-upload/resume-upload.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [ResumeUploadComponent],
  template: `<app-resume-upload />`
})
export class AppComponent {}
