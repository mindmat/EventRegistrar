import { inject, Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { Navigation } from 'app/core/navigation/navigation.types';
import { EventService } from 'app/modules/admin/events/event.service';
import { BehaviorSubject, combineLatest, filter, map, Observable, ReplaySubject, startWith, tap } from 'rxjs';
import { MenuService } from './menu.service';
import { FuseNavigationItem } from '@fuse/components/navigation';
import { MenuNodeContent, MenuNodeKey, MenuNodeStyle } from 'app/api/api';

@Injectable({ providedIn: 'root' })
export class NavigationService
{
    private eventService = inject(EventService);
    private translateService = inject(TranslateService);
    private menuService = inject(MenuService);
    private menu = new BehaviorSubject<FuseNavigationItem[]>(
        [
            {
                id: 'select-event',
                title: 'Event auswählen',
                type: 'basic',
                icon: 'heroicons_outline:arrow-path',
                link: '/select-event',
            }
        ]);

    constructor()
    {
        combineLatest([this.translateService.onLangChange.asObservable().pipe(map(e => e.lang), startWith(this.translateService.currentLang)),
        this.eventService.selected$,
        this.menuService.nodeContents$])
            .pipe(
                filter(([_, e, __]) => e?.acronym != null),
                tap(([_, e, nodes]) =>
                {
                    this.menu.next([
                        {
                            id: 'select-event',
                            title: e.name, // this.translateService.instant('SelectEvent'),
                            type: 'basic',
                            icon: 'heroicons_outline:arrow-path',
                            link: '/select-event'
                        },
                        {
                            id: 'registrations',
                            title: this.translateService.instant('Registrations'),
                            type: 'group',
                            children: [
                                {
                                    id: 'overview',
                                    title: this.translateService.instant('Overview'),
                                    type: 'basic',
                                    icon: 'heroicons_outline:clipboard-document-list',
                                    link: `/${e.acronym}/overview`,
                                },
                                {
                                    id: 'release-mails',
                                    title: this.translateService.instant('ReleaseMails'),
                                    type: 'basic',
                                    icon: 'mat_outline:mail',
                                    link: `/${e.acronym}/mailing/release-mails`,
                                    badge: this.getBadge(nodes, MenuNodeKey.PendingMails)
                                },
                                // {
                                //     id: 'search-registration',
                                //     title: this.translateService.instant('SearchRegistration'),
                                //     type: 'basic',
                                //     icon: 'heroicons_outline:user',
                                //     link: `/${e.acronym}/registrations/search-registration`,
                                // },
                                {
                                    id: 'match-partners',
                                    title: this.translateService.instant('MatchPartners'),
                                    type: 'basic',
                                    icon: 'heroicons_outline:users',
                                    link: `/${e.acronym}/registrations/match-partners`,
                                    badge: this.getBadge(nodes, MenuNodeKey.AssignPartners)
                                },
                                {
                                    id: 'fix-raw-processing',
                                    title: this.translateService.instant('MenuNodeKey_FixRawProcessing'),
                                    type: 'basic',
                                    icon: 'mat_outline:error_outline',
                                    link: `/${e.acronym}/registrations/fix-raw-processing`,
                                    badge: this.getBadge(nodes, MenuNodeKey.FixRawProcessing),
                                    hidden: _ => this.isHidden(nodes, MenuNodeKey.FixRawProcessing)
                                },
                                {
                                    id: 'problematic-emails',
                                    title: this.translateService.instant('MailMonitor'),
                                    type: 'basic',
                                    icon: 'mat_outline:mail',
                                    link: `/${e.acronym}/mailing/problematic-emails`,
                                    badge: this.getBadge(nodes, MenuNodeKey.MailTracking),
                                    hidden: _ => this.isHidden(nodes, MenuNodeKey.MailTracking)
                                },
                                {
                                    id: 'remarks-overview',
                                    title: this.translateService.instant('Remarks'),
                                    type: 'basic',
                                    icon: 'heroicons_outline:chat-bubble-left-ellipsis',
                                    link: `/${e.acronym}/registrations/remarks-overview`,
                                    badge: this.getBadge(nodes, MenuNodeKey.Remarks)
                                },
                                {
                                    id: 'notes-overview',
                                    title: this.translateService.instant('InternalNotes'),
                                    type: 'basic',
                                    icon: 'mat_solid:edit_note',
                                    link: `/${e.acronym}/registrations/notes-overview`,
                                },
                                {
                                    id: 'cancellations',
                                    title: this.translateService.instant('Cancellations'),
                                    type: 'basic',
                                    icon: 'mat_outline:cancel',
                                    link: `/${e.acronym}/registrations/cancellations`,
                                },
                                {
                                    id: 'hosting',
                                    title: this.translateService.instant('Hosting'),
                                    type: 'basic',
                                    icon: 'mat_outline:house',
                                    link: `/${e.acronym}/hosting`,
                                },
                                {
                                    id: 'all-participants',
                                    title: this.translateService.instant('Participants'),
                                    type: 'basic',
                                    icon: 'mat_outline:list',
                                    link: `/${e.acronym}/registrations/all-participants`,
                                },
                                {
                                    id: 'volunteer-planning',
                                    title: this.translateService.instant('VolunteerPlanning'),
                                    type: 'basic',
                                    icon: 'mat_outline:list',
                                    link: `/${e.acronym}/volunteer-planning`,
                                },
                            ]
                        },
                        {
                            id: 'accounting',
                            title: this.translateService.instant('Accounting'),
                            type: 'group',
                            children: [
                                {
                                    id: 'bank-statements',
                                    title: this.translateService.instant('BankStatements'),
                                    type: 'basic',
                                    icon: 'heroicons_outline:currency-dollar',
                                    link: `/${e.acronym}/accounting/bank-statements`,
                                },
                                {
                                    id: 'settle-bookings',
                                    title: this.translateService.instant('AssignBankStatements'),
                                    type: 'basic',
                                    icon: 'heroicons_outline:check',
                                    link: `/${e.acronym}/accounting/settle-payments`,
                                },
                                {
                                    id: 'due-payments',
                                    title: this.translateService.instant('DuePayments'),
                                    type: 'basic',
                                    icon: 'mat_outline:hourglass_bottom',
                                    link: `/${e.acronym}/accounting/due-payments`,
                                    badge: this.getBadge(nodes, MenuNodeKey.DuePayments)
                                },
                                {
                                    id: 'payment-differences',
                                    title: this.translateService.instant('PaymentDifferences'),
                                    type: 'basic',
                                    icon: 'heroicons_outline:arrows-up-down',
                                    link: `/${e.acronym}/accounting/payment-differences`,
                                },
                                {
                                    id: 'payouts',
                                    title: this.translateService.instant('Payouts'),
                                    type: 'basic',
                                    icon: 'heroicons_outline:arrow-right',
                                    link: `/${e.acronym}/accounting/payouts`,
                                }]
                        },
                        {
                            id: 'setup',
                            title: this.translateService.instant('Setup'),
                            type: 'group',
                            icon: 'mat_outline:mail',
                            children: [
                                {
                                    id: 'event-settings',
                                    title: this.translateService.instant('Settings'),
                                    type: 'basic',
                                    icon: 'heroicons_outline:cog-6-tooth',
                                    link: `/${e.acronym}/admin/event-settings`,
                                },
                                {
                                    id: 'setup-event',
                                    title: this.translateService.instant('SetupEvent'),
                                    type: 'basic',
                                    icon: 'heroicons_outline:cog-6-tooth',
                                    link: `/${e.acronym}/admin/setup-event`,
                                },
                                {
                                    id: 'auto-mail-templates',
                                    title: this.translateService.instant('AutoMailTemplates'),
                                    type: 'basic',
                                    icon: 'mat_outline:mail',
                                    link: `/${e.acronym}/mailing/auto-mail-templates`,
                                    badge: this.getBadge(nodes, MenuNodeKey.MailTemplates)
                                },
                                {
                                    id: 'bulk-mail-templates',
                                    title: this.translateService.instant('BulkMailTemplates'),
                                    type: 'basic',
                                    icon: 'mat_outline:mail',
                                    link: `/${e.acronym}/mailing/bulk-mail-templates`,
                                },
                                {
                                    id: 'form-mapping',
                                    title: this.translateService.instant('Forms'),
                                    type: 'basic',
                                    icon: 'heroicons_outline:document-text',
                                    link: `/${e.acronym}/admin/form-mapping`,
                                    badge: this.getBadge(nodes, MenuNodeKey.Forms)
                                },
                                {
                                    id: 'pricing',
                                    title: this.translateService.instant('Pricing'),
                                    type: 'basic',
                                    icon: 'heroicons_outline:currency-euro',
                                    link: `/${e.acronym}/admin/pricing`,
                                },
                            ]
                        },
                    ]);
                }))
            .subscribe();
    }

    get navigation$(): Observable<Navigation>
    {
        return this.menu.pipe(
            map(menu =>
            ({
                default: menu,
                compact: menu,
                horizontal: menu,
                futuristic: menu
            } as Navigation))
        );
    }

    private getBadge(contents: MenuNodeContent[] | null, key: MenuNodeKey): { title: string; classes: string; } | null
    {
        const content = contents?.find(nct => nct.key === key);
        if (!content)
        {
            return null;
        }
        return {
            title: content.content,
            classes: this.getBadgeStyle(content)
        };
    }

    private isHidden(contents: MenuNodeContent[] | null, key: MenuNodeKey): boolean
    {
        const content = contents?.find(nct => nct.key === key);
        if (!content)
        {
            return false;
        }
        return content.hidden === true;
    }

    private getBadgeStyle(content: MenuNodeContent): string | null
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
            case MenuNodeStyle.Important: return 'px-2 bg-red-500 text-black rounded-full';
            default: return 'px-2 bg-sky-600 text-black rounded-full';
        }
    }
}
