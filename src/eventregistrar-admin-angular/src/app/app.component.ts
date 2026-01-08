import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.scss'],
    imports: [RouterOutlet],
})
export class AppComponent {
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
