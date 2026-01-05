import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject, OnDestroy, OnInit, ViewEncapsulation } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Subject, takeUntil } from 'rxjs';
import { VolunteerPlanningService } from '../volunteer-planning.service';
import { ShiftDisplayItem } from 'app/api/api';
import { MomentDateAdapter, MAT_MOMENT_DATE_ADAPTER_OPTIONS } from '@angular/material-moment-adapter';
import { DateAdapter, MAT_DATE_LOCALE, MAT_DATE_FORMATS } from '@angular/material/core';
import * as moment from 'moment';

// See the Moment.js docs for the meaning of these formats:
// https://momentjs.com/docs/#/displaying/format/
export const DE_FORMATS = {
    parse: {
        dateInput: 'DD.MM.YYYY',
    },
    display: {
        dateInput: 'DD.MM.YYYY',
        monthYearLabel: 'DD.MM.YYYY',
        dateA11yLabel: 'DD.MM.YYYY',
        monthYearA11yLabel: 'DD.MM.YYYY',
    },
};

@Component({
    selector: 'app-shift-edit',
    templateUrl: './shift-edit.component.html',
    styleUrls: ['./shift-edit.component.scss'],
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    providers: [
        // `MomentDateAdapter` can be automatically provided by importing `MomentDateModule` in your
        // application's root module. We provide it at the component level here, due to limitations of
        // our example generation script.
        {
            provide: DateAdapter,
            useClass: MomentDateAdapter,
            deps: [MAT_DATE_LOCALE, MAT_MOMENT_DATE_ADAPTER_OPTIONS],
        },
        { provide: MAT_DATE_FORMATS, useValue: DE_FORMATS }
    ]
})
export class ShiftEditComponent implements OnDestroy
{
    shiftForm: FormGroup;
    // shift: any = null;
    shiftId: string = null;
    isNew: boolean = true;
    isLoading: boolean = false;

    private _unsubscribeAll: Subject<any> = new Subject<any>();

    constructor(
        private _formBuilder: FormBuilder,
        private _changeDetectorRef: ChangeDetectorRef,
        private _volunteerPlanningService: VolunteerPlanningService,
        private _matDialogRef: MatDialogRef<ShiftEditComponent>,
        @Inject(MAT_DIALOG_DATA) public data: { shift?: ShiftDisplayItem; }
    )
    {
        const shift = data.shift;
        console.log('ShiftEditComponent data:', shift);

        // Split datetime into date and time components using Moment
        let startDate = null; let startTime = ''; let endDate = null; let endTime = '';

        if (shift?.startTime)
        {
            const startDateTime = moment(shift.startTime);
            startDate = startDateTime;
            startTime = startDateTime.format('HH:mm');
        }

        if (shift?.endTime)
        {
            const endDateTime = moment(shift.endTime);
            endDate = endDateTime;
            endTime = endDateTime.format('HH:mm');
        }

        this.shiftForm = this._formBuilder.group({
            id: [shift?.id || ''],
            name: [shift?.name || '', [Validators.required]],
            description: [shift?.description || ''],
            location: [shift?.location || '', [Validators.required]],
            startDate: [startDate, [Validators.required]],
            startTime: [startTime, [Validators.required]],
            endDate: [endDate, [Validators.required]],
            endTime: [endTime, [Validators.required]],
        });
    }

    // ngOnInit(): void
    // {
    //     // Initialize the form
    //     this.initForm();

    //     // Get data from dialog injection
    //     this.shift = this.data?.shift || null;
    //     this.shiftId = this.data?.shiftId || null;
    //     this.isNew = !this.shift;

    //     if (this.shift)
    //     {
    //         this.populateForm();
    //     }

    //     // If it's a new shift, set the ID in the form data
    //     if (this.isNew && this.shiftId)
    //     {
    //         // Pre-populate with the generated ID
    //         this.shiftForm.patchValue({ id: this.shiftId });
    //     }
    // }

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

        // Combine date and time components back into datetime objects using Moment
        const startMoment = moment(formValue.startDate).set({
            hour: parseInt(formValue.startTime.split(':')[0]),
            minute: parseInt(formValue.startTime.split(':')[1])
        });
        const endMoment = moment(formValue.endDate).set({
            hour: parseInt(formValue.endTime.split(':')[0]),
            minute: parseInt(formValue.endTime.split(':')[1])
        });

        const shiftData = {
            id: formValue.id,
            name: formValue.name,
            description: formValue.description,
            location: formValue.location,
            startTime: startMoment.toDate(),
            endTime: endMoment.toDate()
        } as ShiftDisplayItem;

        // Update shift
        this._volunteerPlanningService.updateShift(formValue.id, shiftData)
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

    /**
     * Close dialog
     */
    closeDialog(): void
    {
        this._matDialogRef.close();
    }

    // private initForm(): void
    // {
    //     this.shiftForm = this._formBuilder.group({
    //         id: [''], // Add ID field for new shifts
    //         name: ['', [Validators.required]],
    //         description: [''],
    //         location: ['', [Validators.required]],
    //         startTime: ['', [Validators.required]],
    //         endTime: ['', [Validators.required]],
    //         helpersNeeded: [1, [Validators.required, Validators.min(1)]]
    //     });
    // }

    // private populateForm(): void
    // {
    //     if (this.shift)
    //     {
    //         // Convert Date objects to datetime-local format for inputs
    //         const startTime = this.shift.startTime ? new Date(this.shift.startTime).toISOString().slice(0, 16) : '';
    //         const endTime = this.shift.endTime ? new Date(this.shift.endTime).toISOString().slice(0, 16) : '';

    //         this.shiftForm.patchValue({
    //             id: this.shift.id,
    //             name: this.shift.name || '',
    //             description: this.shift.description || '',
    //             location: this.shift.location || '',
    //             startTime: startTime,
    //             endTime: endTime,
    //             helpersNeeded: this.shift.helpersNeeded || 1
    //         });
    //     }
    // }
}
