import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, ReplaySubject, tap } from 'rxjs';
import { Navigation } from 'app/core/navigation/navigation.types';
import { FuseNavigationItem } from '@fuse/components/navigation';

@Injectable({
    providedIn: 'root'
})
export class NavigationService
{
    private _navigation: ReplaySubject<Navigation> = new ReplaySubject<Navigation>(1);

    private _staticMenu: FuseNavigationItem[] =
        [
            {
                id: 'overview',
                title: 'Übersicht',
                type: 'basic',
                icon: 'heroicons_outline:clipboard-check',
                link: '/overview',
            },
            {
                id: 'search-registration',
                title: 'Teilnehmer suchen',
                type: 'basic',
                icon: 'heroicons_outline:user',
                link: '/registrations/search-registration',
            },
            {
                id: 'bank-statements',
                title: 'Kontobewegungen',
                type: 'basic',
                icon: 'heroicons_outline:currency-dollar',
                link: '/accounting/bank-statements',
            },
            {
                id: 'settle-bookings',
                title: 'Kontobewegungen zuordnen',
                type: 'basic',
                icon: 'heroicons_outline:check',
                link: '/accounting/settle-payments',
            },
            {
                id: 'due-payments',
                title: 'Ausstehende Zahlungen',
                type: 'basic',
                icon: 'mat_outline:hourglass_bottom',
                link: '/accounting/due-payments',
            },
            {
                id: 'auto-mail-templates',
                title: 'Mailvorlagen',
                type: 'basic',
                icon: 'mat_outline:mail',
                link: '/mailing/auto-mail-templates',
            },
            {
                id: 'release-mails',
                title: 'Mails freigeben',
                type: 'basic',
                icon: 'mat_outline:mail',
                link: '/mailing/release-mails',
            },
            {
                id: 'admin',
                title: 'Administration',
                type: 'collapsable',
                icon: 'mat_outline:mail',
                link: '/mailing/release-mails',
                children: [
                    {
                        id: 'user-access',
                        title: 'Berechtigungen',
                        type: 'basic',
                        icon: 'heroicons_outline:user',
                        link: '/admin/user-access',
                    }
                ]
            },
        ];

    /**
     * Constructor
     */
    constructor(private _httpClient: HttpClient)
    {
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Accessors
    // -----------------------------------------------------------------------------------------------------

    /**
     * Getter for navigation
     */
    get navigation$(): Observable<Navigation>
    {
        return of({
            default: this._staticMenu,
            compact: this._staticMenu,
            horizontal: this._staticMenu,
            futuristic: this._staticMenu
        } as Navigation);

        // return this._navigation.asObservable();
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Get all navigation data
     */
    get(): Observable<Navigation>
    {
        return of({
            default: this._staticMenu,
            compact: this._staticMenu,
            horizontal: this._staticMenu,
            futuristic: this._staticMenu
        } as Navigation);

        // return this._httpClient.get<Navigation>('api/common/navigation').pipe(
        //     tap((navigation) =>
        //     {
        //         this._navigation.next(navigation);
        //     })
        // );
    }
}
