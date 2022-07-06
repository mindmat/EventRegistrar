import { Injectable } from '@angular/core';
import { TranslateLoader } from '@ngx-translate/core';
import { Api } from 'app/api/api';

@Injectable({
    providedIn: 'root'
})
export class TranslationLoaderService
{
    constructor(private api: Api) { }

    createLoader(): TranslateLoader
    {
        return {
            getTranslation: (lang) => this.loadTranslation(lang)
        };
    }

    loadTranslation(lang: string)
    {
        return this.api.translation_Query({ language: lang });
    }
}
