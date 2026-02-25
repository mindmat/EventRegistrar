import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { RequestLogDisplayItem, RequestTypeDisplayItem } from 'app/api/api';
import { Subject, takeUntil } from 'rxjs';
import { EventService } from '../../events/event.service';
import { RequestLogService } from './request-log.service';

@Component({
    standalone: false,
    selector: 'app-request-log',
    templateUrl: './request-log.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class RequestLogComponent implements OnInit, OnDestroy
{
    requestLogs: RequestLogDisplayItem[] = [];
    requestTypeOptions: RequestTypeDisplayItem[] = [];

    searchString = '';
    includeRequestTypes: string[] = [];
    excludeRequestTypes: string[] = [];
    from: Date | null = null;
    to: Date | null = null;
    onlyWithErrors = false;
    onlyUserInitiatedRequests = true;

    private readonly _unsubscribeAll: Subject<any> = new Subject<any>();

    constructor(private readonly _changeDetectorRef: ChangeDetectorRef,
        private readonly eventService: EventService,
        private readonly requestLogService: RequestLogService) { }

    ngOnInit(): void
    {
        this.requestLogService.requestLogs$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe(requestLogs =>
            {
                this.requestLogs = requestLogs;
                this._changeDetectorRef.markForCheck();
            });

        this.requestLogService.fetchRequestTypeOptions()
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe(requestTypeOptions =>
            {
                this.requestTypeOptions = requestTypeOptions;
                this._changeDetectorRef.markForCheck();
            });
    }

    ngOnDestroy(): void
    {
        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }

    applyFilters(): void
    {
        this.requestLogService.fetchRequestLog({
            eventId: this.eventService.selectedId ?? undefined,
            searchString: this.searchString || null,
            includeRequestTypes: this.toNullableArray(this.includeRequestTypes),
            excludeRequestTypes: this.toNullableArray(this.excludeRequestTypes),
            from: this.from,
            to: this.to,
            onlyWithErrors: this.onlyWithErrors ? true : null,
            onlyUserInitiatedRequests: this.onlyUserInitiatedRequests ? true : null
        }).subscribe();
    }

    clearFilters(): void
    {
        this.searchString = '';
        this.includeRequestTypes = [];
        this.excludeRequestTypes = [];
        this.from = null;
        this.to = null;
        this.onlyWithErrors = false;
        this.onlyUserInitiatedRequests = true;

        this.applyFilters();
    }

    trackByFn(index: number, item: RequestLogDisplayItem): any
    {
        return item.id || index;
    }

    private toNullableArray(requestTypes: string[]): string[] | null
    {
        return requestTypes.length > 0 ? requestTypes : null;
    }
}
