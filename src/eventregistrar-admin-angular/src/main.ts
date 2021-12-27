import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { environment } from 'environments/environment';
import { AppModule } from 'app/app.module';

if (environment.production)
{
    enableProdMode();
}
const providers = [
    { provide: 'BASE_API_URL', useValue: 'https://localhost:5001' }
];

platformBrowserDynamic(providers).bootstrapModule(AppModule)
    .catch(err => console.error(err));
