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
            autoInject: false,
            showBranding: false,
            colorScheme: 'light',
            showName: false,
            showEmail: false,
            triggerLabel: 'Feedback'
        }),
        Sentry.replayIntegration({
            maskAllText: false,
            blockAllMedia: false,
            maskAllInputs: false,
        }),
    ],
    // Session Replay
    replaysSessionSampleRate: 0.1, // This sets the sample rate at 10%. You may want to change it to 100% while in development and then sample at a lower rate in production.
    replaysOnErrorSampleRate: 1.0 // If you're not already sampling the entire session, change the sample rate to 100% when sampling sessions where errors occur.
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
