import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit, ViewEncapsulation } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { VolunteerPlanningService } from '../volunteer-planning.service';

@Component({
    selector: 'app-shift-edit',
    templateUrl: './shift-edit.component.html',
    styleUrls: ['./shift-edit.component.scss'],
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ShiftEditComponent implements OnInit, OnDestroy
{
    shiftForm: FormGroup;
    shift: any = null;
    shiftId: string = null;
    isNew: boolean = true;
    isLoading: boolean = false;

    private _unsubscribeAll: Subject<any> = new Subject<any>();

    constructor(
        private _formBuilder: FormBuilder,
        private _activatedRoute: ActivatedRoute,
        private _router: Router,
        private _changeDetectorRef: ChangeDetectorRef,
        private _volunteerPlanningService: VolunteerPlanningService
    ) { }

    ngOnInit(): void
    {
        // Initialize the form
        this.initForm();

        // Get the shift ID from route parameters
        this.shiftId = this._activatedRoute.snapshot.paramMap.get('id');

        // Check if this is a new shift from query parameters
        const isNewFromQuery = this._activatedRoute.snapshot.queryParamMap.get('isNew') === 'true';

        // Get the data
        this._activatedRoute.data
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((data) =>
            {
                this.shift = data.initialData?.shift;

                // Determine if this is new: either no shift data or explicit isNew query param
                this.isNew = !this.shift || isNewFromQuery;

                if (this.shift && !this.isNew)
                {
                    this.populateForm();
                }

                // If it's a new shift, set the ID in the form data
                if (this.isNew && this.shiftId)
                {
                    // Pre-populate with the generated ID
                    this.shiftForm.patchValue({ id: this.shiftId });
                }
            });
    }

    ngOnDestroy(): void
    {
        // Unsubscribe from all subscriptions
        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }

    /**
     * Save shift
     */
    save(): void
    {
        // Return if the form is invalid
        if (this.shiftForm.invalid)
        {
            return;
        }

        // Disable the form
        this.shiftForm.disable();
        this.isLoading = true;

        const formValue = this.shiftForm.getRawValue();

        if (this.isNew)
        {
            // Create shift with pre-generated ID
            const shiftData = {
                ...formValue,
                id: this.shiftId // Use the pre-generated ID
            };

            // this._volunteerPlanningService.createShift(shiftData)
            //     .pipe(takeUntil(this._unsubscribeAll))
            //     .subscribe({
            //         next: (result) =>
            //         {
            //             // Data will be automatically refreshed through NotificationService
            //             this.closeDialog();
            //         },
            //         error: (error) =>
            //         {
            //             this.shiftForm.enable();
            //             this.isLoading = false;
            //             this._changeDetectorRef.markForCheck();
            //         }
            //     });
        } else
        {
            // Update shift
            this._volunteerPlanningService.updateShift(this.shift.id, formValue)
                .pipe(takeUntil(this._unsubscribeAll))
                .subscribe({
                    next: (result) =>
                    {
                        // Data will be automatically refreshed through NotificationService
                        this.closeDialog();
                    },
                    error: (error) =>
                    {
                        this.shiftForm.enable();
                        this.isLoading = false;
                        this._changeDetectorRef.markForCheck();
                    }
                });
        }
    }

    /**
     * Close dialog
     */
    closeDialog(): void
    {
        this._router.navigate(['../'], { relativeTo: this._activatedRoute });
    }

    private initForm(): void
    {
        this.shiftForm = this._formBuilder.group({
            id: [''], // Add ID field for new shifts
            description: ['', [Validators.required]],
            location: ['', [Validators.required]],
            startDate: ['', [Validators.required]],
            endDate: ['', [Validators.required]],
            requiredHelpers: [1, [Validators.required, Validators.min(1)]]
        });
    }

    private populateForm(): void
    {
        if (this.shift)
        {
            this.shiftForm.patchValue({
                id: this.shift.id,
                description: this.shift.description,
                location: this.shift.location,
                startDate: this.shift.startDate,
                endDate: this.shift.endDate,
                requiredHelpers: this.shift.requiredHelpers
            });
        }
    }
}
