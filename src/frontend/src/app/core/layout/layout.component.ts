import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TenantService } from '../services/tenant.service';
import { ProductService } from '../services/product.service';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss']
})
export class LayoutComponent {
  tenantService = inject(TenantService);
  private router = inject(Router);
  private productService = inject(ProductService);
  
  searchQuery = '';

  logout() {
    this.tenantService.clearTenant();
    this.router.navigate(['/login']);
  }

  performSearch() {
    if (this.searchQuery.trim()) {
      // In a real app, you might navigate to a search results page
      // or communicate with the active route via a shared service.
      // For demonstration, we'll just log the semantic search result.
      this.productService.semanticSearch(this.searchQuery).subscribe(results => {
        console.log('Semantic Search Results:', results);
        alert(`Búsqueda IA completada. Se encontraron ${results.length} resultados.`);
      });
    }
  }
}
