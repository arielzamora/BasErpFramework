import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { TenantService } from '../services/tenant.service';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss']
})
export class LayoutComponent {
  tenantService = inject(TenantService);
  private router = inject(Router);

  logout() {
    this.tenantService.clearTenant();
    this.router.navigate(['/login']);
  }
}
