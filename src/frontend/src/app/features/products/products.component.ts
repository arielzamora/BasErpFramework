import { Component, ChangeDetectionStrategy, signal, computed, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GenericTableComponent, TableColumn } from '../../shared/components/generic-table/generic-table.component';
import { TenantService } from '../../core/services/tenant.service';
import { ProductService, Product } from '../../core/services/product.service';
import { SignalrService } from '../../core/services/signalr.service';
import { ToastService } from '../../core/services/toast.service';
import { ModalComponent } from '../../shared/components/modal/modal.component';
import { ProductFormComponent } from './components/product-form/product-form.component';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, GenericTableComponent, ModalComponent, ProductFormComponent],
  templateUrl: './products.component.html',
  styleUrls: ['./products.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductsComponent implements OnInit, OnDestroy {
  private tenantService = inject(TenantService);
  private productService = inject(ProductService);
  private signalrService = inject(SignalrService);
  private toastService = inject(ToastService);
  
  private sub: Subscription | undefined;

  // State
  products = signal<Product[]>([]);
  recentlyModifiedIds = new Set<string>();
  
  // Modal State
  showFormModal = signal(false);
  showDeleteModal = signal(false);
  selectedProduct = signal<Product | null>(null);

  // Derived state based on Tenant
  columns = computed<TableColumn[]>(() => {
    return [
      { key: 'codigo', label: 'Código' },
      { key: 'nombre', label: 'Nombre' },
      { key: 'precio', label: 'Precio', type: 'currency' }
    ];
  });

  ngOnInit() {
    this.loadProducts();

    const tenant = this.tenantService.currentTenant();
    if (tenant) {
      this.signalrService.startConnection(tenant.id);
      
      this.sub = this.signalrService.productUpdates$.subscribe(event => {
        // Prevent echoing SignalR events if we recently modified this product locally
        if (event.payload?.id && this.recentlyModifiedIds.has(event.payload.id)) return;
        if (typeof event.payload === 'string' && this.recentlyModifiedIds.has(event.payload)) return;

        if (event.action === 'ProductoCreated') {
          if (!this.products().some(p => p.id === event.payload.id)) {
            this.products.update(list => [...list, event.payload]);
            this.toastService.show(`Nuevo producto agregado: ${event.payload.nombre}`, 'success');
          }
        } else if (event.action === 'ProductoUpdated') {
          this.products.update(list => list.map(p => p.id === event.payload.id ? event.payload : p));
          this.toastService.show(`Producto actualizado: ${event.payload.nombre}`, 'info');
        } else if (event.action === 'ProductoDeleted') {
          if (this.products().some(p => p.id === event.payload)) {
            this.products.update(list => list.filter(p => p.id !== event.payload));
            this.toastService.show(`Un producto fue eliminado`, 'error');
          }
        }
      });
    }
  }

  ngOnDestroy() {
    this.signalrService.stopConnection();
    if (this.sub) this.sub.unsubscribe();
  }

  loadProducts() {
    this.productService.getAll().subscribe(data => {
      this.products.set(data);
    });
  }

  // --- Actions ---

  openCreateModal() {
    this.selectedProduct.set(null);
    this.showFormModal.set(true);
  }

  handleEdit(product: Product) {
    this.selectedProduct.set(product);
    this.showFormModal.set(true);
  }

  handleDeleteRequest(product: Product) {
    this.selectedProduct.set(product);
    this.showDeleteModal.set(true);
  }

  // --- Callbacks ---

  markAsRecentlyModified(id: string) {
    this.recentlyModifiedIds.add(id);
    setTimeout(() => this.recentlyModifiedIds.delete(id), 2000);
  }

  onSaveProduct(productData: Partial<Product>) {
    const current = this.selectedProduct();
    if (current) {
      // Editar
      this.productService.update(current.id, productData).subscribe(updated => {
        this.markAsRecentlyModified(updated.id);
        this.products.update(list => list.map(p => p.id === updated.id ? updated : p));
        this.showFormModal.set(false);
      });
    } else {
      // Crear
      this.productService.create(productData).subscribe(newProduct => {
        this.markAsRecentlyModified(newProduct.id);
        if (!this.products().some(p => p.id === newProduct.id)) {
          this.products.update(list => [...list, newProduct]);
        }
        this.showFormModal.set(false);
      });
    }
  }

  confirmDelete() {
    const current = this.selectedProduct();
    if (current) {
      this.markAsRecentlyModified(current.id);
      this.productService.delete(current.id).subscribe(() => {
        this.products.update(list => list.filter(p => p.id !== current.id));
        this.showDeleteModal.set(false);
        this.selectedProduct.set(null);
      });
    }
  }
}
