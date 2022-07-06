import { Injectable } from '@angular/core';
import { MissingTranslationHandler, MissingTranslationHandlerParams } from '@ngx-translate/core';

@Injectable({
  providedIn: 'root'
})
export class MissingTranslationService implements MissingTranslationHandler
{
  constructor() { }

  handle(params: MissingTranslationHandlerParams)
  {
    // if (!environment.production) 
    {
      console.warn(`translation not found for key ${params.key}`);
    }

    return `[${params.key}]`;
  }
}
