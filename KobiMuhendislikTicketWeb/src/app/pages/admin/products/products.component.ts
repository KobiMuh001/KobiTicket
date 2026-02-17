import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CreateProductDto, ProductListItem, ProductService, UpdateProductDto } from '../../../core/services/product.service';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './products.component.html',
  styleUrls: ['./products.component.scss']
})
export class ProductsComponent implements OnInit {
  products: ProductListItem[] = [];
  filteredProducts: ProductListItem[] = [];
  isLoading = true;
  errorMessage = '';
  searchTerm = '';
  showCreateModal = false;
  showEditModal = false;
  showDeleteModal = false;
  isSubmitting = false;
  selectedProductId: number | null = null;

  newProduct: CreateProductDto = {
    name: '',
    description: ''
  };

  editProduct: UpdateProductDto = {
    name: '',
    description: ''
  };

  constructor(
    private productService: ProductService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.isLoading = true;
    this.productService.getProducts().subscribe({
      next: (response) => {
        this.products = response?.data || [];
        this.applyFilter();
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Ürünler yüklenirken bir hata oluştu.';
        this.isLoading = false;
      }
    });
  }

  applyFilter(): void {
    const term = this.searchTerm.trim().toLowerCase();

    if (!term) {
      this.filteredProducts = [...this.products];
      return;
    }

    this.filteredProducts = this.products.filter(product =>
      product.name.toLowerCase().includes(term) ||
      (product.description || '').toLowerCase().includes(term)
    );
  }

  onSearch(): void {
    this.applyFilter();
  }

  openProduct(productId: number): void {
    this.router.navigate(['/admin/products', productId]);
  }

  openEditModal(product: ProductListItem, event?: Event): void {
    event?.stopPropagation();
    this.selectedProductId = product.id;
    this.editProduct = {
      name: product.name,
      description: product.description || ''
    };
    this.errorMessage = '';
    this.showEditModal = true;
  }

  closeEditModal(): void {
    this.showEditModal = false;
    this.selectedProductId = null;
  }

  updateProduct(): void {
    if (!this.selectedProductId) return;

    if (!this.editProduct.name?.trim()) {
      this.errorMessage = 'Ürün adı zorunludur.';
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    this.productService.updateProduct(this.selectedProductId, this.editProduct).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.closeEditModal();
        this.loadProducts();
      },
      error: (error) => {
        this.isSubmitting = false;
        this.errorMessage = error?.error?.message || 'Ürün güncellenirken bir hata oluştu.';
      }
    });
  }

  openDeleteModal(product: ProductListItem, event?: Event): void {
    event?.stopPropagation();
    this.selectedProductId = product.id;
    this.editProduct = {
      name: product.name,
      description: product.description || ''
    };
    this.errorMessage = '';
    this.showDeleteModal = true;
  }

  closeDeleteModal(): void {
    this.showDeleteModal = false;
    this.selectedProductId = null;
  }

  deleteProduct(): void {
    if (!this.selectedProductId) return;

    this.isSubmitting = true;
    this.errorMessage = '';

    this.productService.deleteProduct(this.selectedProductId).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.closeDeleteModal();
        this.loadProducts();
      },
      error: (error) => {
        this.isSubmitting = false;
        this.errorMessage = error?.error?.message || 'Ürün silinirken bir hata oluştu.';
      }
    });
  }

  openCreateModal(): void {
    this.newProduct = {
      name: '',
      description: ''
    };
    this.errorMessage = '';
    this.showCreateModal = true;
  }

  closeCreateModal(): void {
    this.showCreateModal = false;
  }

  createProduct(): void {
    if (!this.newProduct.name?.trim()) {
      this.errorMessage = 'Ürün adı zorunludur.';
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    this.productService.createProduct(this.newProduct).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.showCreateModal = false;
        this.loadProducts();
      },
      error: (error) => {
        this.isSubmitting = false;
        this.errorMessage = error?.error?.message || 'Ürün eklenirken bir hata oluştu.';
      }
    });
  }
}
