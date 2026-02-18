import { Component, inject, OnInit } from '@angular/core';
import { Auth } from '../../service/auth';
import { Router } from '@angular/router';
import { DashboardService } from '../../service/dashboardService';
import { CommonModule } from '@angular/common';
import { SummaryResponse } from '../../interface/summaryResponse';
import { ChartDataInterface } from '../../interface/chart-data';
import { ViewChild, ElementRef } from '@angular/core';
import { Chart, ChartConfiguration, ChartData, registerables } from 'chart.js';
@Component({
  selector: 'app-dashboard',
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit {
  isSidebarOpen = false;
  userEmail = '';
  summaryData: SummaryResponse | null = null; // يمكنك تحديد نوع البيانات المناسب هنا

  // يمكنك تحديد نوع البيانات المناسب هنا
  toggleSidebar() {
    this.isSidebarOpen = !this.isSidebarOpen;
  }
  authService = inject(Auth)
  dashboatdService = inject(DashboardService)
  router = inject(Router)
  chartData?: ChartData;
  loading = true;

  @ViewChild('salesChart') chartCanvas!: ElementRef;
  chart!: Chart;

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
  ngOnInit(): void {
    if (!this.authService.isLoggedIn()) {
      this.logout();
    }
    if (!this.authService.isAdmin()) {
      this.router.navigate(['/login']);
    }
    this.dashboatdService.getSummary(new Date().getMonth() + 1, new Date().getFullYear()).subscribe({
      next: (res) => {
        this.summaryData = res;
        console.log(res);
      },
      error: (err) => {
        console.error(err);
      },

    })
      ;
    this.dashboatdService.getChartData().subscribe(data => {
      this.createChart(data);
    });
    this.userEmail = this.authService.getuserEmail() || '';
  }
  createChart(apiData: any) {
    const ctx = this.chartCanvas.nativeElement.getContext('2d');

    // عمل التدرج اللوني الأخضر للمبيعات
    const salesGradient = ctx.createLinearGradient(0, 0, 0, 400);
    salesGradient.addColorStop(0, 'rgba(5, 150, 105, 0.2)');
    salesGradient.addColorStop(1, 'rgba(5, 150, 105, 0)');

    this.chart = new Chart(ctx, {
      type: 'line',
      data: {
        labels: apiData.days, // مثلاً [1, 2, 3, ..., 30]
        datasets: [
          {
            label: 'المبيعات',
            data: apiData.salesValues,
            borderColor: '#059669',
            borderWidth: 3,
            fill: true,
            backgroundColor: salesGradient,
            tension: 0.4, // لجعل الخط انسيابي (Curve)
            pointRadius: 0,
            pointHoverRadius: 6,
            pointHoverBackgroundColor: '#fff',
            pointHoverBorderColor: '#059669',
            pointHoverBorderWidth: 3
          },
          {
            label: 'المصروفات',
            data: apiData.expenseValues,
            borderColor: '#94a3b8',
            borderWidth: 2,
            borderDash: [5, 5], // خط مقطع كما في التصميم
            fill: false,
            tension: 0.4,
            pointRadius: 0
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } }, // سنستخدم الـ Legend اليدوي الخاص بك
        scales: {
          y: { beginAtZero: true, grid: { display: true } },
          x: { grid: { display: false } }
        }
      }
    });
  }
}
