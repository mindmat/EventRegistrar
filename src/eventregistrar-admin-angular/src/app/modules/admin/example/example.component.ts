import { HttpClient } from '@angular/common/http';
import { Component, ViewEncapsulation } from '@angular/core';
import { map, Observable, shareReplay, tap } from 'rxjs';
import { MatButton } from '@angular/material/button';

@Component({
    selector: 'example',
    templateUrl: './example.component.html',
    encapsulation: ViewEncapsulation.None
})
export class ExampleComponent
{
    result$: Observable<Object>;
    /**
     * Constructor
     */
    constructor(private http: HttpClient)
    {
    }

    callApi()
    {
        console.log('call API');
        this.result$ = this.http.get('https://localhost:5001/api/me/events?includeRequestedEvents=true');
    }
}
