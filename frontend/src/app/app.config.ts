import { ApplicationConfig } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';

// Angular 17+ standalone bootstrap config
export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient()   // makes HttpClient available app-wide
  ]
};
