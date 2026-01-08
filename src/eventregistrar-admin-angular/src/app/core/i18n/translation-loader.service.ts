import { Injectable } from '@angular/core';
import { TranslateLoader } from '@ngx-translate/core';
import { Api } from 'app/api/api';

@Injectable({
    providedIn: 'root'
})
export class TranslationLoaderService implements TranslateLoader
{
    constructor(private api: Api) { }

    getTranslation(lang: string)
    {
        return this.api.translation_Query({ language: lang });
    }
}
