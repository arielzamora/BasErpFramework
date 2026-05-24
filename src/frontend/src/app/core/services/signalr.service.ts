import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';

export interface ProductUpdateEvent {
  action: string;
  payload: any;
}

@Injectable({
  providedIn: 'root'
})
export class SignalrService {
  private hubConnection: signalR.HubConnection | undefined;
  
  // Subject to emit events to components
  public productUpdates$ = new Subject<ProductUpdateEvent>();

  public startConnection(tenantId: string) {
    const envUrl = (window as any).env?.apiUrl;
    const baseHubUrl = envUrl ? `${envUrl}/hubs/producto` : 'https://localhost:7197/hubs/producto';
    
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${baseHubUrl}?tenantId=${tenantId}`)
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => {
        console.log('SignalR Connection Started');
        this.hubConnection?.invoke('JoinTenantGroup', tenantId);
      })
      .catch(err => console.error('Error while starting SignalR connection: ' + err));

    this.addListeners();
  }

  public stopConnection() {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }

  private addListeners() {
    if (!this.hubConnection) return;

    this.hubConnection.on('OnProductoUpdate', (data: any) => {
      console.log('Received SignalR event:', data);
      this.productUpdates$.next({ action: data.action, payload: data.payload });
    });
  }
}
