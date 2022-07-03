import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit, ViewEncapsulation } from '@angular/core';
import { MatSlideToggleChange } from '@angular/material/slide-toggle';
import { BehaviorSubject, combineLatest, Subject, takeUntil } from 'rxjs';
import { MatSelectChange } from '@angular/material/select';
import { OverviewService } from './overview.service';
import { RegistrableTagDisplayItem } from '../registrables/tags/registrableTagDisplayItem';
import { DoubleRegistrableDisplayItem, SingleRegistrableDisplayItem } from 'app/api/api';

@Component({
    selector: 'app-overview',
    templateUrl: './overview.component.html',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class OverviewComponent implements OnInit, OnDestroy
{
    tags: RegistrableTagDisplayItem[];
    singleRegistrables: SingleRegistrableDisplayItem[];
    doubleRegistrables: DoubleRegistrableDisplayItem[];
    filteredSingleRegistrables: SingleRegistrableDisplayItem[];
    filteredDoubleRegistrables: DoubleRegistrableDisplayItem[];
    filters: {
        categoryTag$: BehaviorSubject<string>;
        query$: BehaviorSubject<string>;
        hideCompleted$: BehaviorSubject<boolean>;
    } = {
            categoryTag$: new BehaviorSubject('all'),
            query$: new BehaviorSubject(''),
            hideCompleted$: new BehaviorSubject(false)
        };

    private _unsubscribeAll: Subject<any> = new Subject<any>();

    constructor(private _changeDetectorRef: ChangeDetectorRef, private overviewService: OverviewService) { }

    ngOnInit(): void
    {
        // Get the tags
        this.overviewService.registrableTags$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((tags: RegistrableTagDisplayItem[]) =>
            {
                this.tags = tags;

                // Mark for check
                this._changeDetectorRef.markForCheck();
            });

        // Get the registrables
        this.overviewService.singleRegistrables$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((singleRegistrables: SingleRegistrableDisplayItem[]) =>
            {
                this.singleRegistrables = this.filteredSingleRegistrables = singleRegistrables;

                // Mark for check
                this._changeDetectorRef.markForCheck();
            });

        this.overviewService.doubleRegistrables$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((doubleRegistrables: DoubleRegistrableDisplayItem[]) =>
            {
                this.doubleRegistrables = this.filteredDoubleRegistrables = doubleRegistrables;

                // Mark for check
                this._changeDetectorRef.markForCheck();
            });

        // Filter the courses
        combineLatest([this.filters.categoryTag$, this.filters.query$, this.filters.hideCompleted$])
            .subscribe(([categoryTag, query, hideCompleted]) =>
            {

                // Reset the filtered courses
                this.filteredSingleRegistrables = this.singleRegistrables;
                this.filteredDoubleRegistrables = this.doubleRegistrables;

                // Filter by category
                if (categoryTag !== 'all')
                {
                    this.filteredSingleRegistrables = this.filteredSingleRegistrables.filter(course => course.tag === categoryTag);
                    this.filteredDoubleRegistrables = this.filteredDoubleRegistrables.filter(course => course.tag === categoryTag);
                }

                // Filter by search query
                if (query !== '')
                {
                    this.filteredSingleRegistrables = this.filteredSingleRegistrables.filter(rbl => rbl.name.toLowerCase().includes(query.toLowerCase()) || rbl.nameSecondary?.toLowerCase().includes(query.toLowerCase()));
                    this.filteredDoubleRegistrables = this.filteredDoubleRegistrables.filter(rbl => rbl.name.toLowerCase().includes(query.toLowerCase()) || rbl.nameSecondary?.toLowerCase().includes(query.toLowerCase()));
                }

                // Filter by completed
                if (hideCompleted)
                {
                    // this.filteredCourses = this.filteredCourses.filter(course => course.progress.completed === 0);
                }
            });
    }

    ngOnDestroy(): void
    {
        // Unsubscribe from all subscriptions
        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }

    filterByQuery(query: string): void
    {
        this.filters.query$.next(query);
    }

    filterByCategory(change: MatSelectChange): void
    {
        this.filters.categoryTag$.next(change.value);
    }

    toggleCompleted(change: MatSlideToggleChange): void
    {
        this.filters.hideCompleted$.next(change.checked);
    }

    trackByFn(index: number, item: any): any
    {
        return item.id || index;
    }
}
