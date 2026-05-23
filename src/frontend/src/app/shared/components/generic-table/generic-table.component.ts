import { Component, EventEmitter, Input, Output, ChangeDetectionStrategy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface TableColumn {
  key: string;
  label: string;
  type?: 'text' | 'currency' | 'boolean' | 'badge' | 'custom';
}

@Component({
  selector: 'app-generic-table',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './generic-table.component.html',
  styleUrls: ['./generic-table.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class GenericTableComponent {
  @Input() columns: TableColumn[] = [];
  @Input() set data(value: any[]) {
    this._data.set(value || []);
    this.currentPage.set(1);
  }
  @Input() title: string = '';
  
  @Output() edit = new EventEmitter<any>();
  @Output() delete = new EventEmitter<any>();

  // State
  private _data = signal<any[]>([]);
  searchQuery = signal('');
  sortColumn = signal<string | null>(null);
  sortDirection = signal<'asc' | 'desc'>('asc');
  currentPage = signal(1);
  pageSize = signal(5); // Default 5 items per page

  // Computed Pipelines
  filteredAndSortedData = computed(() => {
    let result = this._data();
    
    // 1. Search Filter
    const query = this.searchQuery().toLowerCase().trim();
    if (query) {
      result = result.filter(item => {
        return Object.values(item).some(val => 
          val !== null && val !== undefined && val.toString().toLowerCase().includes(query)
        );
      });
    }

    // 2. Sort
    const col = this.sortColumn();
    if (col) {
      const direction = this.sortDirection() === 'asc' ? 1 : -1;
      result = [...result].sort((a, b) => {
        const valA = a[col];
        const valB = b[col];
        
        if (valA === valB) return 0;
        if (valA == null) return 1;
        if (valB == null) return -1;
        
        if (typeof valA === 'string' && typeof valB === 'string') {
          return valA.localeCompare(valB) * direction;
        }
        
        return (valA < valB ? -1 : 1) * direction;
      });
    }

    return result;
  });

  paginatedData = computed(() => {
    const data = this.filteredAndSortedData();
    const page = this.currentPage();
    const size = this.pageSize();
    const startIndex = (page - 1) * size;
    return data.slice(startIndex, startIndex + size);
  });

  totalPages = computed(() => Math.max(1, Math.ceil(this.filteredAndSortedData().length / this.pageSize())));

  // Actions
  onSearchChange(event: Event) {
    const target = event.target as HTMLInputElement;
    this.searchQuery.set(target.value);
    this.currentPage.set(1); // Reset to page 1 on search
  }

  toggleSort(columnKey: string) {
    if (this.sortColumn() === columnKey) {
      if (this.sortDirection() === 'asc') {
        this.sortDirection.set('desc');
      } else {
        this.sortColumn.set(null); // Remove sort on 3rd click
        this.sortDirection.set('asc');
      }
    } else {
      this.sortColumn.set(columnKey);
      this.sortDirection.set('asc');
    }
    this.currentPage.set(1); // Reset to page 1 on sort
  }

  nextPage() {
    if (this.currentPage() < this.totalPages()) {
      this.currentPage.update(p => p + 1);
    }
  }

  prevPage() {
    if (this.currentPage() > 1) {
      this.currentPage.update(p => p - 1);
    }
  }
  
  changePageSize(event: Event) {
    const target = event.target as HTMLSelectElement;
    this.pageSize.set(Number(target.value));
    this.currentPage.set(1);
  }

  onEdit(item: any) { this.edit.emit(item); }
  onDelete(item: any) { this.delete.emit(item); }
}
