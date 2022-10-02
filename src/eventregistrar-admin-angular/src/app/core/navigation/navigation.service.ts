import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, map, Observable, of, ReplaySubject, switchMap, tap } from 'rxjs';
import { Navigation } from 'app/core/navigation/navigation.types';
import { FuseNavigationItem } from '@fuse/components/navigation';
import { EventService } from 'app/modules/admin/events/event.service';

@Injectable({
    providedIn: 'root'
})
export class NavigationService
{
    private _navigation: ReplaySubject<Navigation> = new ReplaySubject<Navigation>(1);

    private menu = new BehaviorSubject<FuseNavigationItem[]>(
        [
            {
                id: 'select-event',
                title: 'Event auswählen',
                type: 'basic',
                icon: 'heroicons_outline:clipboard-check',
                link: `/select-event`,
            }
        ]);

    /**
     * Constructor
     */
    constructor(private _httpClient: HttpClient,
        eventService: EventService)
    {
        eventService.selected$.subscribe(e =>
        {
            this.menu.next([
                {
                    id: 'select-event',
                    title: 'Event auswählen',
                    type: 'basic',
                    icon: 'heroicons_outline:clipboard-check',
                    link: `/select-event`,
                },
                {
                    id: 'overview',
                    title: 'Übersicht',
                    type: 'basic',
                    icon: 'heroicons_outline:clipboard-check',
                    link: `/${e.acronym}/overview`,
                },
                {
                    id: 'search-registration',
                    title: 'Teilnehmer suchen',
                    type: 'basic',
                    icon: 'heroicons_outline:user',
                    link: `/${e.acronym}/registrations/search-registration`,
                },
                {
                    id: 'bank-statements',
                    title: 'Kontobewegungen',
                    type: 'basic',
                    icon: 'heroicons_outline:currency-dollar',
                    link: `/${e.acronym}/accounting/bank-statements`,
                },
                {
                    id: 'settle-bookings',
                    title: 'Kontobewegungen zuordnen',
                    type: 'basic',
                    icon: 'heroicons_outline:check',
                    link: `/${e.acronym}/accounting/settle-payments`,
                },
                {
                    id: 'due-payments',
                    title: 'Ausstehende Zahlungen',
                    type: 'basic',
                    icon: 'mat_outline:hourglass_bottom',
                    link: `/${e.acronym}/accounting/due-payments`,
                },
                {
                    id: 'auto-mail-templates',
                    title: 'Mailvorlagen',
                    type: 'basic',
                    icon: 'mat_outline:mail',
                    link: `/${e.acronym}/mailing/auto-mail-templates`,
                },
                {
                    id: 'release-mails',
                    title: 'Mails freigeben',
                    type: 'basic',
                    icon: 'mat_outline:mail',
                    link: `/${e.acronym}/mailing/release-mails`,
                },
                {
                    id: 'admin',
                    title: 'Administration',
                    type: 'collapsable',
                    icon: 'mat_outline:mail',
                    children: [
                        {
                            id: 'user-access',
                            title: 'Berechtigungen',
                            type: 'basic',
                            icon: 'heroicons_outline:user',
                            link: `/${e.acronym}/admin/user-access`,
                        }
                    ]
                },
            ]);
        });
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Accessors
    // -----------------------------------------------------------------------------------------------------

    /**
     * Getter for navigation
     */
    get navigation$(): Observable<Navigation>
    {
        return this.menu.pipe(
            map(menu =>
            {
                return {
                    default: menu,
                    compact: menu,
                    horizontal: menu,
                    futuristic: menu
                } as Navigation;
            })
        );
        // return of({
        //     default: this.menu,
        //     compact: this.menu,
        //     horizontal: this.menu,
        //     futuristic: this.menu
        // } as Navigation);

        // return this._navigation.asObservable();
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Get all navigation data
     */
    // get(): Observable<Navigation>
    // {
    //     return of({
    //         default: this.menu,
    //         compact: this.menu,
    //         horizontal: this.menu,
    //         futuristic: this.menu
    //     } as Navigation);

    //     // return this._httpClient.get<Navigation>('api/common/navigation').pipe(
    //     //     tap((navigation) =>
    //     //     {
    //     //         this._navigation.next(navigation);
    //     //     })
    //     // );
    // }
}
