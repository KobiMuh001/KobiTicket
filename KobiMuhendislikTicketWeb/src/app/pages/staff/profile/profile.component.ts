import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StaffService } from '../../../core/services/staff.service';

@Component({
  selector: 'app-staff-profile',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class StaffProfileComponent implements OnInit {
  profile: any = null;
  workload: any = null;
  isLoading = true;

  constructor(private staffService: StaffService) {}

  ngOnInit(): void {
    this.loadProfile();
    this.loadWorkload();
  }

  loadProfile(): void {
    this.staffService.getMyProfile().subscribe({
      next: (res) => {
        if (res.success) {
          this.profile = res.data;
        }
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  loadWorkload(): void {
    this.staffService.getMyWorkload().subscribe({
      next: (res) => {
        if (res.success) {
          this.workload = res.data;
        }
      }
    });
  }
}
