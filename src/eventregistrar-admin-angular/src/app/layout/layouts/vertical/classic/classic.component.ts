import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit, ViewEncapsulation } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { FuseMediaWatcherService } from '@fuse/services/media-watcher';
import { FuseNavigationService, FuseVerticalNavigationComponent } from '@fuse/components/navigation';
import { Navigation } from 'app/core/navigation/navigation.types';
import { NavigationService } from 'app/core/navigation/navigation.service';
import { NotificationService } from 'app/modules/admin/infrastructure/notification.service';
import * as Sentry from "@sentry/angular";
import { UserService } from 'app/core/user/user.service';
import { TranslateService } from '@ngx-translate/core';

@Component({
    selector: 'classic-layout',
    templateUrl: './classic.component.html',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ClassicLayoutComponent implements OnInit, OnDestroy
{
    isScreenSmall: boolean;
    navigation: Navigation;
    public isConnected: boolean;
    private _unsubscribeAll: Subject<any> = new Subject<any>();

    /**
     * Constructor
     */
    constructor(
        private _activatedRoute: ActivatedRoute,
        private _router: Router,
        private _navigationService: NavigationService,
        private _fuseMediaWatcherService: FuseMediaWatcherService,
        private _fuseNavigationService: FuseNavigationService,
        private _notificationService: NotificationService,
        private _changeDetectorRef: ChangeDetectorRef,
        private _userService: UserService,
        private _translateService: TranslateService
    )
    {
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Accessors
    // -----------------------------------------------------------------------------------------------------

    /**
     * Getter for current year
     */
    get currentYear(): number
    {
        return new Date().getFullYear();
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Lifecycle hooks
    // -----------------------------------------------------------------------------------------------------

    /**
     * On init
     */
    ngOnInit(): void
    {
        // Subscribe to navigation data
        this._navigationService.navigation$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((navigation: Navigation) =>
            {
                this.navigation = navigation;
            });

        // Subscribe to media changes
        this._fuseMediaWatcherService.onMediaChange$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe(({ matchingAliases }) =>
            {

                // Check if the screen is small
                this.isScreenSmall = !matchingAliases.includes('md');
            });

        this._notificationService.isConnected$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe(isConnected =>
            {
                this.isConnected = isConnected;
                this._changeDetectorRef.markForCheck();
                console.log(`isConnected: ${isConnected}`);
            });
    }

    triggerReconnect()
    {
        this._notificationService.reconnect();
    }

    async sendFeedback()
    {
        const feedback = Sentry.getFeedback();
        const form = await feedback?.createForm({
            showName: false,
            showEmail: false,
            messageLabel: this._translateService.instant('Feedback'),
            isRequiredLabel: this._translateService.instant('Required'),
            addScreenshotButtonLabel: this._translateService.instant('AddScreenshot'),
            removeScreenshotButtonLabel: this._translateService.instant('RemoveScreenshot'),
            triggerLabel: this._translateService.instant('Trigger'),
            cancelButtonLabel: this._translateService.instant('Cancel'),
            submitButtonLabel: this._translateService.instant('Submit'),
            messagePlaceholder: '',// this._translateService.instant('FeedbackPlaceholder'),
            formTitle: this._translateService.instant('FeedbackTitle'),
            successMessageText: this._translateService.instant('FeedbackSuccess'),

            showBranding: false,
            colorScheme: 'light',
            useSentryUser: { name: 'username', email: 'email' },
        });
        form.appendToDom();
        form.open();
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
     * Toggle navigation
     *
     * @param name
     */
    toggleNavigation(name: string): void
    {
        // Get the navigation
        const navigation = this._fuseNavigationService.getComponent<FuseVerticalNavigationComponent>(name);

        if (navigation)
        {
            // Toggle the opened status
            navigation.toggle();
        }
    }
}
