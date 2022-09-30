import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { environment } from 'environments/environment';
import { AppModule } from 'app/app.module';
import { API_BASE_URL } from 'app/api/api';

if (environment.production)
{
    enableProdMode();
}
const providers = [
    { provide: API_BASE_URL, useValue: 'https://localhost:5001' }
];

platformBrowserDynamic(providers).bootstrapModule(AppModule)
    .catch(err => console.error(err));
