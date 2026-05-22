import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { TenantService, Tenant } from '../../core/services/tenant.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  private tenantService = inject(TenantService);
  private router = inject(Router);

  tenants: Tenant[] = [
    { id: 'tenant-1', name: 'TechComponents S.A.', type: 'standard' },
    { id: 'tenant-2', name: 'Logística Quilmes', type: 'isolated' },
    { id: 'tenant-3', name: 'MegaCorp Enterprise AI-Powered', type: 'premium' }
  ];

  selectTenant(tenant: Tenant) {
    this.tenantService.setTenant(tenant);
    this.router.navigate(['/app']);
  }
}
