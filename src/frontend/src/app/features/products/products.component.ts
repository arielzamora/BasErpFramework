import { Component, ChangeDetectionStrategy, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GenericTableComponent, TableColumn } from '../../shared/components/generic-table/generic-table.component';
import { TenantService } from '../../core/services/tenant.service';

export interface Product {
  id: string;
  code: string;
  name: string;
  price: number;
  isActive: boolean;
  aiMetadata?: string; // Only populated for premium tenant
}

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, GenericTableComponent],
  templateUrl: './products.component.html',
  styleUrls: ['./products.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductsComponent {
  private tenantService = inject(TenantService);
  
  // State
  products = signal<Product[]>([
    { id: '1', code: 'PRD-001', name: 'Notebook Pro 15', price: 1200, isActive: true, aiMetadata: 'High demand predicted' },
    { id: '2', code: 'PRD-002', name: 'Ergonomic Chair', price: 350, isActive: true, aiMetadata: 'Standard rotation' },
    { id: '3', code: 'PRD-003', name: 'Mechanical Keyboard', price: 150, isActive: false, aiMetadata: 'Low stock warning' }
  ]);

  // Derived state based on Tenant
  columns = computed<TableColumn[]>(() => {
    const baseCols: TableColumn[] = [
      { key: 'code', label: 'Código' },
      { key: 'name', label: 'Nombre' },
      { key: 'price', label: 'Precio', type: 'currency' },
      { key: 'isActive', label: 'Activo', type: 'boolean' }
    ];

    const currentTenant = this.tenantService.currentTenant();
    if (currentTenant?.type === 'premium') {
      baseCols.push({ key: 'aiMetadata', label: 'IA Insights', type: 'badge' });
    }

    return baseCols;
  });

  handleEdit(product: Product) {
    alert(`Editar producto: ${product.name}`);
    // Here we would open an Update Modal
  }

  handleDelete(product: Product) {
    if (confirm(`¿Estás seguro de eliminar el producto ${product.name}?`)) {
      this.products.update(list => list.filter(p => p.id !== product.id));
    }
  }

  openCreateModal() {
    alert('Abrir modal de creación de producto');
    // Here we would open a Create Modal
  }
}
