import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AssetService } from '../../../core/services/asset.service';

interface Asset {
  id: string;
  name: string;
  description: string;
  serialNumber: string;
  status: string;
  createdAt: string;
}

@Component({
  selector: 'app-customer-assets',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './customer-assets.component.html',
  styleUrls: ['./customer-assets.component.scss']
})
export class CustomerAssetsComponent implements OnInit {
  assets: Asset[] = [];
  filteredAssets: Asset[] = [];
  isLoading = true;
  searchQuery = '';

  constructor(private assetService: AssetService) {}

  ngOnInit(): void {
    this.loadAssets();
  }

  loadAssets(): void {
    this.isLoading = true;
    
    this.assetService.getMyAssets().subscribe({
      next: (response: any) => {
        const data = response.data || response || [];
        this.assets = data.map((a: any) => ({
          id: a.id,
          name: a.productName || a.name,
          description: a.description,
          serialNumber: a.serialNumber,
          status: this.getStatusText(a.status),
          createdAt: a.createdDate || a.createdAt
        }));
        this.applyFilter();
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  applyFilter(): void {
    if (!this.searchQuery) {
      this.filteredAssets = [...this.assets];
    } else {
      const query = this.searchQuery.toLowerCase();
      this.filteredAssets = this.assets.filter(asset =>
        asset.name.toLowerCase().includes(query) ||
        asset.serialNumber.toLowerCase().includes(query) ||
        asset.description?.toLowerCase().includes(query)
      );
    }
  }

  getStatusText(status: string | number): string {
    const statusMap: { [key: string]: string; [key: number]: string } = {
      'Active': 'Aktif',
      'Inactive': 'Pasif',
      'UnderMaintenance': 'Bakımda',
      'Retired': 'Kullanım Dışı',
      0: 'Aktif',
      1: 'Pasif',
      2: 'Bakımda',
      3: 'Kullanım Dışı'
    };
    return statusMap[status] || status.toString();
  }

  getStatusClass(status: string): string {
    const classMap: { [key: string]: string } = {
      'Aktif': 'status-active',
      'Pasif': 'status-inactive',
      'Bakımda': 'status-maintenance',
      'Kullanım Dışı': 'status-retired'
    };
    return classMap[status] || '';
  }
}
