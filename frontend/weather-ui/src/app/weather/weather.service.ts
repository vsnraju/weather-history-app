import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { WeatherEntry } from './weather-entry';

@Injectable({
  providedIn: 'root'
})
export class WeatherService {
  private readonly httpClient = inject(HttpClient);

  getWeather(): Observable<WeatherEntry[]> {
    return this.httpClient
      .get<WeatherEntry[]>('/api/weather')
      .pipe(map((entries) => entries ?? []));
  }
}
