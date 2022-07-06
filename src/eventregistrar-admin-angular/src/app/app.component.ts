import { Component } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.scss']
})
export class AppComponent
{
    /**
     * Constructor
     */
    constructor(translateService: TranslateService)
    {
        translateService.addLangs(['de', 'en']);
        var language = window.localStorage.getItem('language') ?? 'de';
        translateService.use(language);
    }
}
