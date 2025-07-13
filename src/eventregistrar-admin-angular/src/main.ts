import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { environment } from 'environments/environment';
import { AppModule } from 'app/app.module';
import { API_BASE_URL } from 'app/api/api';
import * as Sentry from "@sentry/angular";

Sentry.init({
    dsn: "https://6750b676b29dc0515e939ac7e7093280@o4509662088790016.ingest.de.sentry.io/4509662090428496",
    // Setting this option to true will send default PII data to Sentry.
    // For example, automatic IP address collection on events
    sendDefaultPii: true,
    integrations: [
        Sentry.feedbackIntegration({
            showBranding: false,
            colorScheme: 'light',
            showName: false,
            showEmail: false
        })
    ],

});

if (environment.production)
{
    enableProdMode();
}
const providers = [
    { provide: API_BASE_URL, useValue: environment.API_BASE_URL }
];

platformBrowserDynamic(providers).bootstrapModule(AppModule)
    .catch(err => console.error(err));
