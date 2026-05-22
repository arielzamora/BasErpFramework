import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { TenantService } from '../services/tenant.service';

export const tenantInterceptor: HttpInterceptorFn = (req, next) => {
  const tenantService = inject(TenantService);
  const tenant = tenantService.currentTenant();

  if (tenant) {
    // Clone the request and add the X-Tenant-ID header
    const modifiedReq = req.clone({
      headers: req.headers.set('X-Tenant-ID', tenant.id)
    });
    return next(modifiedReq);
  }

  return next(req);
};
