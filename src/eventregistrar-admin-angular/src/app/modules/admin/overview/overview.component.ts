import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit, ViewEncapsulation } from '@angular/core';
import { BehaviorSubject, combineLatest, Subject, takeUntil } from 'rxjs';
import { OverviewService } from './overview.service';
import { RegistrableTagDisplayItem } from '../registrables/tags/registrableTagDisplayItem';
import { DoubleRegistrableDisplayItem, RegistrablesOverview, SingleRegistrableDisplayItem } from 'app/api/api';
import { MatSelectChange } from '@angular/material/select';
import { MatSlideToggleChange } from '@angular/material/slide-toggle';
import { MatDialog } from '@angular/material/dialog';
import { RegistrableDetailComponent } from './registrable-detail/registrable-detail.component';
import { RegistrablesService } from '../pricing/registrables.service';

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

    private unsubscribeAll: Subject<any> = new Subject<any>();

    constructor(private changeDetectorRef: ChangeDetectorRef,
        private overviewService: OverviewService,
        private registrableService: RegistrablesService,
        private matDialog: MatDialog) { }

    ngOnInit(): void
    {
        // Get the tags
        this.overviewService.registrableTags$
            .pipe(takeUntil(this.unsubscribeAll))
            .subscribe((tags: RegistrableTagDisplayItem[]) =>
            {
                this.tags = tags;

                // Mark for check
                this.changeDetectorRef.markForCheck();
            });

        // Get the registrables
        this.overviewService.registrables$
            .pipe(takeUntil(this.unsubscribeAll))
            .subscribe((registrables: RegistrablesOverview) =>
            {
                this.singleRegistrables = this.filteredSingleRegistrables = registrables.singleRegistrables;
                this.doubleRegistrables = this.filteredDoubleRegistrables = registrables.doubleRegistrables;

                // Mark for check
                this.changeDetectorRef.markForCheck();
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

    addRegistrable(): void
    {
        this.matDialog.open(RegistrableDetailComponent, {
            autoFocus: true,
            data: { singleRegistrable: null, doubleRegistrable: null }
        });
    }

    ngOnDestroy(): void
    {
        // Unsubscribe from all subscriptions
        this.unsubscribeAll.next(null);
        this.unsubscribeAll.complete();
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

    changeDoubleRegistrable(doubleRegistrable: DoubleRegistrableDisplayItem)
    {
        this.matDialog.open(RegistrableDetailComponent, {
            autoFocus: true,
            data: { singleRegistrable: null, doubleRegistrable }
        });
    }

    changeSingleRegistrable(singleRegistrable: SingleRegistrableDisplayItem)
    {
        this.matDialog.open(RegistrableDetailComponent, {
            autoFocus: true,
            data: { singleRegistrable, doubleRegistrable: null }
        });
    }

    deleteRegistrable(registrableId: string)
    {
        this.registrableService.deleteRegistrable(registrableId);
    }

    trackByFn(index: number, item: any): any
    {
        return item.id || index;
    }
}
