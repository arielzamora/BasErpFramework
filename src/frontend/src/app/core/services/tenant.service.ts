import { Injectable, signal } from '@angular/core';

export interface Tenant {
  id: string;
  name: string;
  type: 'standard' | 'isolated' | 'premium';
}

@Injectable({
  providedIn: 'root'
})
export class TenantService {
  // Signal to store the currently selected tenant
  public currentTenant = signal<Tenant | null>(null);

  constructor() {
    // Optionally recover tenant from localStorage on app init
    const savedTenant = localStorage.getItem('bas_tenant');
    if (savedTenant) {
      try {
        this.currentTenant.set(JSON.parse(savedTenant));
      } catch (e) {
        console.error('Failed to parse saved tenant');
      }
    }
  }

  setTenant(tenant: Tenant) {
    this.currentTenant.set(tenant);
    localStorage.setItem('bas_tenant', JSON.stringify(tenant));
  }

  clearTenant() {
    this.currentTenant.set(null);
    localStorage.removeItem('bas_tenant');
  }
}
