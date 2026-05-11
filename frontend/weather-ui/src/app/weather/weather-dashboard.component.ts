import { CommonModule } from '@angular/common';
import { Component, DestroyRef, OnInit, ViewChild, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { debounceTime, finalize, startWith } from 'rxjs';
import { WeatherEntry } from './weather-entry';
import { WeatherService } from './weather.service';

@Component({
  selector: 'app-weather-dashboard',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatChipsModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSortModule,
    MatTableModule,
    MatToolbarModule,
    MatTooltipModule
  ],
  templateUrl: './weather-dashboard.component.html',
  styleUrl: './weather-dashboard.component.scss'
})
export class WeatherDashboardComponent implements OnInit {
  private readonly weatherService = inject(WeatherService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly displayedColumns = [
    'date',
    'minTemperatureC',
    'maxTemperatureC',
    'precipitationMm',
    'status'
  ];
  protected readonly dataSource = new MatTableDataSource<WeatherEntry>([]);
  protected readonly filterControl = new FormControl('', { nonNullable: true });
  protected readonly isLoading = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly selectedEntry = signal<WeatherEntry | null>(null);
  protected readonly totalCount = signal(0);
  protected readonly invalidCount = signal(0);
  protected readonly cachedCount = signal(0);
  protected readonly loadedCount = computed(() => this.totalCount() - this.invalidCount());

  @ViewChild(MatSort)
  private set sort(sort: MatSort | undefined) {
    if (sort) {
      this.dataSource.sort = sort;
    }
  }

  ngOnInit(): void {
    this.dataSource.filterPredicate = (entry, filter) =>
      [
        entry.input,
        entry.date,
        entry.status,
        entry.errorMessage,
        this.formatTemperature(entry.minTemperatureC),
        this.formatTemperature(entry.maxTemperatureC),
        this.formatPrecipitation(entry.precipitationMm)
      ]
        .join(' ')
        .toLowerCase()
        .includes(filter);

    this.dataSource.sortingDataAccessor = (entry, property) => {
      switch (property) {
        case 'date':
          return entry.date ?? entry.input;
        case 'minTemperatureC':
          return entry.minTemperatureC ?? Number.NEGATIVE_INFINITY;
        case 'maxTemperatureC':
          return entry.maxTemperatureC ?? Number.NEGATIVE_INFINITY;
        case 'precipitationMm':
          return entry.precipitationMm ?? Number.NEGATIVE_INFINITY;
        default:
          return String(entry[property as keyof WeatherEntry] ?? '');
      }
    };

    this.filterControl.valueChanges
      .pipe(startWith(this.filterControl.value), debounceTime(150), takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        this.dataSource.filter = value.trim().toLowerCase();
      });

    this.loadWeather();
  }

  protected loadWeather(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.selectedEntry.set(null);

    this.weatherService
      .getWeather()
      .pipe(finalize(() => this.isLoading.set(false)), takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (entries) => {
          this.dataSource.data = entries;
          this.totalCount.set(entries.length);
          this.invalidCount.set(entries.filter((entry) => entry.status === 'InvalidDate').length);
          this.cachedCount.set(entries.filter((entry) => entry.isCached).length);
        },
        error: () => {
          this.dataSource.data = [];
          this.totalCount.set(0);
          this.invalidCount.set(0);
          this.cachedCount.set(0);
          this.errorMessage.set('Weather data could not be loaded. Confirm the API is running and try again.');
        }
      });
  }

  protected clearFilter(): void {
    this.filterControl.setValue('');
  }

  protected selectEntry(entry: WeatherEntry): void {
    this.selectedEntry.set(entry);
  }

  protected formatTemperature(value: number | null): string {
    return value === null ? 'Unavailable' : `${value.toFixed(1)} C`;
  }

  protected formatPrecipitation(value: number | null): string {
    return value === null ? 'Unavailable' : `${value.toFixed(1)} mm`;
  }
}
