export interface WeatherEntry {
  input: string;
  date: string | null;
  minTemperatureC: number | null;
  maxTemperatureC: number | null;
  precipitationMm: number | null;
  status: 'Cached' | 'Fetched' | 'InvalidDate' | 'WeatherUnavailable' | 'StorageFailed' | string;
  errorMessage: string | null;
  isCached: boolean;
}
