import { Injectable } from '@angular/core';
import { BehaviorSubject, combineLatest, filter, map, Observable, of, ReplaySubject, startWith, switchMap, tap } from 'rxjs';
import { Navigation } from 'app/core/navigation/navigation.types';
import { FuseNavigationItem } from '@fuse/components/navigation';
import { EventService } from 'app/modules/admin/events/event.service';
import { TranslateService } from '@ngx-translate/core';
import { Api, MenuNodeContent, MenuNodeKey, MenuNodeStyle } from 'app/api/api';
import { MenuService } from './menu.service';

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
        translateService: TranslateService,
        menuService: MenuService)
    {
        combineLatest([translateService.onLangChange.asObservable().pipe(map(e => e.lang), startWith(translateService.currentLang)),
        eventService.selected$,
        menuService.nodeContents$])
            .pipe(
                filter(([_, e, __]) => e?.acronym != null),
                tap(([_, e, nodes]) =>
                {
                    this.menu.next([
                        {
                            id: 'select-event',
                            title: e.name, // translateService.instant('SelectEvent'),
                            type: 'basic',
                            icon: 'heroicons_outline:clipboard-check',
                            link: `/select-event`
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
                                    key: MenuNodeKey.PendingMails,
                                    title: translateService.instant('ReleaseMails'),
                                    type: 'basic',
                                    icon: 'mat_outline:mail',
                                    link: `/${e.acronym}/mailing/release-mails`,
                                    badge: this.getBadge(nodes, MenuNodeKey.PendingMails)
                                },
                                // {
                                //     id: 'search-registration',
                                //     title: translateService.instant('SearchRegistration'),
                                //     type: 'basic',
                                //     icon: 'heroicons_outline:user',
                                //     link: `/${e.acronym}/registrations/search-registration`,
                                // },
                                {
                                    id: 'match-partners',
                                    title: translateService.instant('MatchPartners'),
                                    type: 'basic',
                                    icon: 'heroicons_outline:users',
                                    link: `/${e.acronym}/registrations/match-partners`,
                                    badge: this.getBadge(nodes, MenuNodeKey.AssignPartners)
                                },
                                {
                                    id: 'problematic-emails',
                                    title: translateService.instant('MailMonitor'),
                                    type: 'basic',
                                    icon: 'mat_outline:mail',
                                    link: `/${e.acronym}/mailing/problematic-emails`,
                                },
                                {
                                    id: 'remarks-overview',
                                    title: translateService.instant('Remarks'),
                                    type: 'basic',
                                    icon: 'heroicons_outline:chat-alt',
                                    link: `/${e.acronym}/registrations/remarks-overview`,
                                    badge: this.getBadge(nodes, MenuNodeKey.Remarks)
                                },
                                {
                                    id: 'notes-overview',
                                    title: translateService.instant('InternalNotes'),
                                    type: 'basic',
                                    icon: 'mat_solid:edit_note',
                                    link: `/${e.acronym}/registrations/notes-overview`,
                                },
                                {
                                    id: 'cancellations',
                                    title: translateService.instant('Cancellations'),
                                    type: 'basic',
                                    icon: 'mat_outline:cancel',
                                    link: `/${e.acronym}/registrations/cancellations`,
                                },
                                {
                                    id: 'hosting',
                                    title: translateService.instant('Hosting'),
                                    type: 'basic',
                                    icon: 'mat_outline:house',
                                    link: `/${e.acronym}/hosting`,
                                },
                                {
                                    id: 'all-participants',
                                    title: translateService.instant('Participants'),
                                    type: 'basic',
                                    icon: 'mat_outline:list',
                                    link: `/${e.acronym}/registrations/all-participants`,
                                },
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
                                    badge: this.getBadge(nodes, MenuNodeKey.DuePayments)
                                },
                                {
                                    id: 'payment-differences',
                                    title: translateService.instant('PaymentDifferences'),
                                    type: 'basic',
                                    icon: 'heroicons_outline:switch-vertical',
                                    link: `/${e.acronym}/accounting/payment-differences`,
                                },
                                {
                                    id: 'payouts',
                                    title: translateService.instant('Payouts'),
                                    type: 'basic',
                                    icon: 'heroicons_outline:arrow-right',
                                    link: `/${e.acronym}/accounting/payouts`,
                                }]
                        },
                        {
                            id: 'setup',
                            title: translateService.instant('Setup'),
                            type: 'group',
                            icon: 'mat_outline:mail',
                            children: [
                                {
                                    id: 'event-settings',
                                    title: translateService.instant('Settings'),
                                    type: 'basic',
                                    icon: 'heroicons_outline:cog-8-tooth',
                                    link: `/${e.acronym}/admin/event-settings`,
                                },
                                {
                                    id: 'setup-event',
                                    title: translateService.instant('SetupEvent'),
                                    type: 'basic',
                                    icon: 'heroicons_outline:cog-6-tooth',
                                    link: `/${e.acronym}/admin/setup-event`,
                                },
                                {
                                    id: 'auto-mail-templates',
                                    title: translateService.instant('AutoMailTemplates'),
                                    type: 'basic',
                                    icon: 'mat_outline:mail',
                                    link: `/${e.acronym}/mailing/auto-mail-templates`,
                                },
                                {
                                    id: 'bulk-mail-templates',
                                    title: translateService.instant('BulkMailTemplates'),
                                    type: 'basic',
                                    icon: 'mat_outline:mail',
                                    link: `/${e.acronym}/mailing/bulk-mail-templates`,
                                },
                                {
                                    id: 'form-mapping',
                                    title: translateService.instant('Forms'),
                                    type: 'basic',
                                    icon: 'heroicons_outline:clipboard-list',
                                    link: `/${e.acronym}/admin/form-mapping`,
                                },
                                {
                                    id: 'pricing',
                                    title: translateService.instant('Pricing'),
                                    type: 'basic',
                                    icon: 'heroicons_outline:cash',
                                    link: `/${e.acronym}/admin/pricing`,
                                },
                            ]
                        },
                    ]);
                }))
            .subscribe();
    }

    private getBadge(contents: MenuNodeContent[] | null, key: MenuNodeKey): { title: string, classes: string; } | null
    {
        var content = contents?.find(nct => nct.key === key);
        if (!content)
        {
            return null;
        }
        return {
            title: content.content,
            classes: this.getBadgeStyle(content)
        };
    }

    getBadgeStyle(content: MenuNodeContent): string | null
    {
        if (!content.content)
        {
            // avoid empty badge
            return null;
        }
        switch (content.style)
        {
            case MenuNodeStyle.Info: return 'px-2 bg-sky-600 text-black rounded-full';
            case MenuNodeStyle.ToDo: return 'px-2 bg-yellow-500 text-black rounded-full';
            default: return 'px-2 bg-sky-600 text-black rounded-full';
        }
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
