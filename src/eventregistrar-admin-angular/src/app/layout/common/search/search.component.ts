import { ChangeDetectorRef, Component, ElementRef, EventEmitter, HostBinding, Input, OnChanges, OnDestroy, OnInit, Output, Renderer2, SimpleChanges, ViewChild, ViewEncapsulation } from '@angular/core';
import { UntypedFormControl } from '@angular/forms';
import { debounceTime, filter, map, Subject, takeUntil } from 'rxjs';
import { fuseAnimations } from '@fuse/animations/public-api';
import { Api, RegistrationMatch, RegistrationState } from 'app/api/api';
import { EventService } from 'app/modules/admin/events/event.service';
import { MatAutocomplete, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { NavigatorService } from 'app/modules/admin/navigator.service';
import { TranslateService } from '@ngx-translate/core';

@Component({
    selector: 'search',
    templateUrl: './search.component.html',
    encapsulation: ViewEncapsulation.None,
    exportAs: 'fuseSearch',
    animations: fuseAnimations
})
export class SearchComponent implements OnChanges, OnInit, OnDestroy
{
    @Input() appearance: 'basic' | 'bar' = 'basic';
    @Input() debounce: number = 300;
    @Input() minLength: number = 2;
    @Input() navigate: boolean = true;
    @Output() search: EventEmitter<any> = new EventEmitter<any>();
    @Output() selected: EventEmitter<SearchResult> = new EventEmitter<SearchResult>();

    opened: boolean = false;
    resultSets: SearchResultSet[];
    searchControl: UntypedFormControl = new UntypedFormControl();
    private _matAutocomplete: MatAutocomplete;
    private _unsubscribeAll: Subject<any> = new Subject<any>();

    /**
     * Constructor
     */
    constructor(
        private api: Api,
        private eventService: EventService,
        private changeDetector: ChangeDetectorRef,
        private translateService: TranslateService,
        public navigator: NavigatorService) { }

    // -----------------------------------------------------------------------------------------------------
    // @ Accessors
    // -----------------------------------------------------------------------------------------------------

    /**
     * Host binding for component classes
     */
    @HostBinding('class') get classList(): any
    {
        return {
            'search-appearance-bar': this.appearance === 'bar',
            'search-appearance-basic': this.appearance === 'basic',
            'search-opened': this.opened
        };
    }

    /**
     * Setter for bar search input
     *
     * @param value
     */
    @ViewChild('barSearchInput')
    set barSearchInput(value: ElementRef)
    {
        // If the value exists, it means that the search input
        // is now in the DOM, and we can focus on the input..
        if (value)
        {
            // Give Angular time to complete the change detection cycle
            setTimeout(() =>
            {

                // Focus to the input element
                value.nativeElement.focus();
            });
        }
    }

    /**
     * Setter for mat-autocomplete element reference
     *
     * @param value
     */
    @ViewChild('matAutocomplete')
    set matAutocomplete(value: MatAutocomplete)
    {
        this._matAutocomplete = value;
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Lifecycle hooks
    // -----------------------------------------------------------------------------------------------------

    /**
     * On changes
     *
     * @param changes
     */
    ngOnChanges(changes: SimpleChanges): void
    {
        // Appearance
        if ('appearance' in changes)
        {
            // To prevent any issues, close the
            // search after changing the appearance
            this.close();
        }
    }

    /**
     * On init
     */
    ngOnInit(): void
    {
        // Subscribe to the search field value changes
        this.searchControl.valueChanges
            .pipe(
                debounceTime(this.debounce),
                takeUntil(this._unsubscribeAll),
                map((value) =>
                {

                    // Set the resultSets to null if there is no value or
                    // the length of the value is smaller than the minLength
                    // so the autocomplete panel can be closed
                    if (!value || value.length < this.minLength)
                    {
                        this.resultSets = null;
                    }

                    // Continue
                    return value;
                }),
                // Filter out undefined/null/false statements and also
                // filter out the values that are smaller than minLength
                filter(value => value && value.length >= this.minLength)
            )
            .subscribe((value) =>
            {
                this.api.searchRegistration_Query({ eventId: this.eventService.selectedId, searchString: value, states: [RegistrationState.Received, RegistrationState.Paid, RegistrationState.Cancelled] })
                    .subscribe((resultSets: RegistrationMatch[]) =>
                    {
                        // Store the result sets
                        this.resultSets = [{
                            id: 'registrations',
                            label: this.translateService.instant('Registrations'),
                            results: resultSets.map(match =>
                            ({
                                avatar: null,
                                name: `${match.firstName} ${match.lastName}`,
                                link: null,
                                value: match.registrationId,
                                strikethrough: match.state === RegistrationState.Cancelled
                            } as SearchResult))
                        }];

                        // Execute the event
                        this.search.next(resultSets);

                        this.changeDetector.markForCheck();
                    });
            });
    }

    selectOption(e: MatAutocompleteSelectedEvent)
    {
        const item: SearchResult = e.option.value;
        if (this.navigate)
        {
            this.navigator.goToRegistration(item.value);
            this.close();
            this.resultSets = null;
        }
        else
        {
            this.selected.next(item);
        }
    }

    displayResult(option: SearchResult)
    {
        return option?.name;
    }

    /**
     * On destroy
     */
    ngOnDestroy(): void
    {
        // Unsubscribe from all subscriptions
        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * On keydown of the search input
     *
     * @param event
     */
    onKeydown(event: KeyboardEvent): void
    {
        // Escape
        if (event.code === 'Escape')
        {
            // If the appearance is 'bar' and the mat-autocomplete is not open, close the search
            if (this.appearance === 'bar' && !this._matAutocomplete.isOpen)
            {
                this.close();
            }
        }
    }

    /**
     * Open the search
     * Used in 'bar'
     */
    open(): void
    {
        // Return if it's already opened
        if (this.opened)
        {
            return;
        }

        // Open the search
        this.opened = true;
    }

    /**
     * Close the search
     * * Used in 'bar'
     */
    close(): void
    {
        // Return if it's already closed
        if (!this.opened)
        {
            return;
        }

        // Clear the search input
        this.searchControl.setValue('');

        // Close the search
        this.opened = false;
    }

    /**
     * Track by function for ngFor loops
     *
     * @param index
     * @param item
     */
    trackByFn(index: number, item: any): any
    {
        return item.id || index;
    }
}

export class SearchResultSet
{
    id: string;
    label: string;
    results: SearchResult[];
}

export class SearchResult
{
    avatar: string;
    name: string;
    link: string | null;
    value: string | null;
    strikethrough: boolean;
}