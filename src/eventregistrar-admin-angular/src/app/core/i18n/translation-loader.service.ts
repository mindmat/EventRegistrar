import { Injectable, InjectionToken } from '@angular/core';
import { TranslateLoader } from '@ngx-translate/core';
import { Api } from 'app/api/api';
import { of } from 'rxjs';

export const DEFAULT_LANG = 'default';
// export const LANGUAGE = new InjectionToken<Language>('LANGUAGE');

@Injectable({
    providedIn: 'root'
})
export class TranslationLoaderService
{

    constructor(private api: Api)//, private service: LanguageService)
    {

    }

    createLoader(): TranslateLoader
    {
        return {
            getTranslation: () => this.loadTranslation()
        };
    }

    loadTranslation()
    {
        // if (this.service.showKeys)
        // {
        //     return of({});
        // }

        return this.api.translation_Query({ language: "de" });
    }
}
