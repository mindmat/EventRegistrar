import { Injectable } from '@angular/core';
import { BehaviorSubject, combineLatest, filter, map, Observable, of, ReplaySubject, startWith, switchMap, tap } from 'rxjs';
import { Navigation } from 'app/core/navigation/navigation.types';
import { FuseNavigationItem } from '@fuse/components/navigation';
import { EventService } from 'app/modules/admin/events/event.service';
import { TranslateService } from '@ngx-translate/core';

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
    constructor(eventService: EventService,
        translateService: TranslateService)
    {
        combineLatest([translateService.onLangChange.asObservable().pipe(map(e => e.lang), startWith(translateService.currentLang)), eventService.selected$])
            .pipe(
                filter(([_, e]) => e?.acronym != null),
                tap(([_, e]) =>
                {
                    this.menu.next([
                        {
                            id: 'select-event',
                            title: translateService.instant('SelectEvent'),
                            type: 'basic',
                            icon: 'heroicons_outline:clipboard-check',
                            link: `/select-event`,
                        },
                        {
                            id: 'registrations',
                            title: translateService.instant('Registrations'),
                            type: 'group',
                            children: [
                                {
                                    id: 'overview',
                                    title: translateService.instant('Overview'),
                                    type: 'basic',
                                    icon: 'heroicons_outline:clipboard-check',
                                    link: `/${e.acronym}/overview`,
                                },
                                {
                                    id: 'release-mails',
                                    title: translateService.instant('ReleaseMails'),
                                    type: 'basic',
                                    icon: 'mat_outline:mail',
                                    link: `/${e.acronym}/mailing/release-mails`,
                                },
                                {
                                    id: 'search-registration',
                                    title: translateService.instant('SearchRegistration'),
                                    type: 'basic',
                                    icon: 'heroicons_outline:user',
                                    link: `/${e.acronym}/registrations/search-registration`,
                                }
                            ]
                        },
                        {
                            id: 'accounting',
                            title: translateService.instant('Accounting'),
                            type: 'group',
                            children: [
                                {
                                    id: 'bank-statements',
                                    title: translateService.instant('BankStatement'),
                                    type: 'basic',
                                    icon: 'heroicons_outline:currency-dollar',
                                    link: `/${e.acronym}/accounting/bank-statements`,
                                },
                                {
                                    id: 'settle-bookings',
                                    title: translateService.instant('AssignBankStatements'),
                                    type: 'basic',
                                    icon: 'heroicons_outline:check',
                                    link: `/${e.acronym}/accounting/settle-payments`,
                                },
                                {
                                    id: 'due-payments',
                                    title: translateService.instant('DuePayments'),
                                    type: 'basic',
                                    icon: 'mat_outline:hourglass_bottom',
                                    link: `/${e.acronym}/accounting/due-payments`,
                                }]
                        },
                        {
                            id: 'setup',
                            title: translateService.instant('Setup'),
                            type: 'group',
                            icon: 'mat_outline:mail',
                            children: [
                                {
                                    id: 'user-access',
                                    title: translateService.instant('AccessRights'),
                                    type: 'basic',
                                    icon: 'heroicons_outline:user',
                                    link: `/${e.acronym}/admin/user-access`,
                                },
                                {
                                    id: 'form-mapping',
                                    title: translateService.instant('Forms'),
                                    type: 'basic',
                                    icon: 'heroicons_outline:clipboard-list',
                                    link: `/${e.acronym}/admin/form-mapping`,
                                },
                                {
                                    id: 'auto-mail-templates',
                                    title: translateService.instant('AutoMailTemplates'),
                                    type: 'basic',
                                    icon: 'mat_outline:mail',
                                    link: `/${e.acronym}/mailing/auto-mail-templates`,
                                }
                            ]
                        },
                    ]);
                }))
            .subscribe();
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
