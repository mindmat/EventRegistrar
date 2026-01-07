import { ModuleWithProviders, NgModule, inject, provideAppInitializer } from '@angular/core';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { FUSE_MOCK_API_DEFAULT_DELAY } from '@fuse/lib/mock-api/mock-api.constants';
import { FuseMockApiInterceptor } from '@fuse/lib/mock-api/mock-api.interceptor';

@NgModule({
    providers: [
        {
            provide : HTTP_INTERCEPTORS,
            useClass: FuseMockApiInterceptor,
            multi   : true
        }
    ]
})
export class FuseMockApiModule
{
    /**
     * FuseMockApi module default configuration.
     *
     * @param mockApiServices - Array of services that register mock API handlers
     * @param config - Configuration options
     * @param config.delay - Default delay value in milliseconds to apply all responses
     */
    static forRoot(mockApiServices: any[], config?: { delay?: number }): ModuleWithProviders<FuseMockApiModule>
    {
        return {
            ngModule : FuseMockApiModule,
            providers: [
                provideAppInitializer(() => {
        const initializerFn = (() => (): any => null)(inject(...mockApiServices));
        return initializerFn();
      }),
                {
                    provide : FUSE_MOCK_API_DEFAULT_DELAY,
                    useValue: config?.delay ?? 0
                }
            ]
        };
    }
}
