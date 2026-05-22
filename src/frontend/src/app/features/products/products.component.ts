import { Component, ChangeDetectionStrategy, signal, computed, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GenericTableComponent, TableColumn } from '../../shared/components/generic-table/generic-table.component';
import { TenantService } from '../../core/services/tenant.service';
import { ProductService, Product } from '../../core/services/product.service';
import { SignalrService } from '../../core/services/signalr.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, GenericTableComponent],
  templateUrl: './products.component.html',
  styleUrls: ['./products.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductsComponent implements OnInit, OnDestroy {
  private tenantService = inject(TenantService);
  private productService = inject(ProductService);
  private signalrService = inject(SignalrService);
  
  private sub: Subscription | undefined;

  // State
  products = signal<Product[]>([]);

  // Derived state based on Tenant
  columns = computed<TableColumn[]>(() => {
    const baseCols: TableColumn[] = [
      { key: 'codigo', label: 'Código' },
      { key: 'nombre', label: 'Nombre' },
      { key: 'precio', label: 'Precio', type: 'currency' }
    ];

    const currentTenant = this.tenantService.currentTenant();
    if (currentTenant?.type === 'premium') {
      // Could display semantic search badges or ai metadata here if returned
      // baseCols.push({ key: 'aiMetadata', label: 'IA Insights', type: 'badge' });
    }

    return baseCols;
  });

  ngOnInit() {
    this.loadProducts();

    const tenant = this.tenantService.currentTenant();
    if (tenant) {
      this.signalrService.startConnection(tenant.id);
      
      this.sub = this.signalrService.productUpdates$.subscribe(event => {
        if (event.action === 'ProductoCreated') {
          this.products.update(list => [...list, event.payload]);
        } else if (event.action === 'ProductoUpdated') {
          this.products.update(list => list.map(p => p.id === event.payload.id ? event.payload : p));
        } else if (event.action === 'ProductoDeleted') {
          this.products.update(list => list.filter(p => p.id !== event.payload));
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

  handleEdit(product: Product) {
    const newPrice = prompt(`Editar precio para ${product.nombre}:`, product.precio.toString());
    if (newPrice && !isNaN(Number(newPrice))) {
      this.productService.update(product.id, { ...product, precio: Number(newPrice) }).subscribe();
    }
  }

  handleDelete(product: Product) {
    if (confirm(`¿Estás seguro de eliminar el producto ${product.nombre}?`)) {
      this.productService.delete(product.id).subscribe();
    }
  }

  openCreateModal() {
    const nombre = prompt('Nombre del nuevo producto:');
    if (!nombre) return;
    const codigo = prompt('Código del producto:');
    if (!codigo) return;
    const precioStr = prompt('Precio:');
    if (!precioStr || isNaN(Number(precioStr))) return;

    this.productService.create({
      nombre,
      codigo,
      precio: Number(precioStr)
    }).subscribe();
  }
}
