import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

/**
 * Güvenli logger servisi - sadece development modunda log yapar
 * Production'da hassas hata detayları açığa çıkmaz
 */
@Injectable({
  providedIn: 'root'
})
export class LoggerService {
  
  error(message: string, ...optionalParams: any[]): void {
    if (!environment.production) {
      console.error(message, ...optionalParams);
    }
  }

  warn(message: string, ...optionalParams: any[]): void {
    if (!environment.production) {
      console.warn(message, ...optionalParams);
    }
  }

  log(message: string, ...optionalParams: any[]): void {
    if (!environment.production) {
      console.log(message, ...optionalParams);
    }
  }

  debug(message: string, ...optionalParams: any[]): void {
    if (!environment.production) {
      console.debug(message, ...optionalParams);
    }
  }
}
