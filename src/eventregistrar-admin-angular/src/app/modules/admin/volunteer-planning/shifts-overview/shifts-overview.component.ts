import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit, ViewEncapsulation } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { Subject, takeUntil, Observable } from 'rxjs';
import { FuseConfirmationService } from '@fuse/services/confirmation';
import { VolunteerPlanningService } from '../volunteer-planning.service';
import { ShiftAssignmentsComponent } from '../shift-assignments/shift-assignments.component';
import { ParticipantDisplayItem, ShiftDisplayItem } from 'app/api/api';
import { v4 as createUuid } from 'uuid';

@Component({
    selector: 'app-shifts-overview',
    templateUrl: './shifts-overview.component.html',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ShiftsOverviewComponent implements OnInit, OnDestroy
{
    shifts$: Observable<ShiftDisplayItem[]>;
    currentShifts: ShiftDisplayItem[] = [];
    availableParticipants: ParticipantDisplayItem[] = [];
    maxHelpers: number = 3; // Maximum number of helper columns to show
    maxHelpersNeeded: number = 3; // Dynamic maximum based on shifts data
    private _unsubscribeAll: Subject<any> = new Subject<any>();

    constructor(
        private _activatedRoute: ActivatedRoute,
        private _router: Router,
        private _changeDetectorRef: ChangeDetectorRef,
        private _dialog: MatDialog,
        private _fuseConfirmationService: FuseConfirmationService,
        private _volunteerPlanningService: VolunteerPlanningService
    ) { }

    ngOnInit(): void
    {
        // Subscribe to shifts$ observable for reactive updates
        this.shifts$ = this._volunteerPlanningService.shifts$;

        // Subscribe to shifts to keep currentShifts updated
        this.shifts$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((shifts) =>
            {
                this.currentShifts = shifts || [];
                this.maxHelpersNeeded = Math.max(...this.currentShifts.map(s => s.helpersNeeded || 0), 0);
                this._changeDetectorRef.markForCheck();
            });

        // Trigger initial data fetch
        this._volunteerPlanningService.fetchShifts()
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe();

        // Load available participants for dropdowns
        this.loadAvailableParticipants();
    }

    ngOnDestroy(): void
    {
        // Unsubscribe from all subscriptions
        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }

    /**
     * Get array for helper column iteration
     */
    getHelperColumns(): number[]
    {
        return Array.from({ length: this.maxHelpersNeeded }, (_, i) => i + 1);
    }

    /**
     * Create new shift automatically using the last shift's end time
     */
    createShift(): void
    {
        let lastShift: ShiftDisplayItem;

        if (this.currentShifts && this.currentShifts.length > 0)
        {
            // Use the last shift's end time as start time for new shift
            lastShift = this.currentShifts[this.currentShifts.length - 1];
        }

        const startTime = lastShift?.endTime ?? new Date();
        // Calculate end time (1 hour after start time by default)
        const endTime = new Date(startTime);
        endTime.setHours(endTime.getHours() + 1);

        // Call the service to create the shift
        this._volunteerPlanningService.createShift(
            lastShift?.location,
            startTime,
            endTime);
    }



    /**
     * Edit shift
     */
    editShift(shiftId: string): void
    {
        this._router.navigate([`./${shiftId}/edit`], { relativeTo: this._activatedRoute });
    }

    /**
     * Add helper slot to shift
     */
    addHelperSlot(shift: ShiftDisplayItem): void
    {
        this._volunteerPlanningService.addHelperSlot(shift.id)
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe({
                next: () =>
                {
                    console.log('Helper slot added successfully');
                },
                error: (error) =>
                {
                    console.error('Error adding helper slot:', error);
                }
            });
    }

    /**
     * Assign participant to responsible role
     */
    assignResponsible(shiftId: string, participantId: string): void
    {
        if (participantId)
        {
            this._volunteerPlanningService.assignToShift(shiftId, participantId, true)
                .pipe(takeUntil(this._unsubscribeAll))
                .subscribe(() =>
                {
                    // Data will be automatically refreshed through NotificationService
                });
        }
    }

    /**
     * Assign participant to helper role
     */
    assignHelper(shiftId: string, participantId: string, helperIndex: number): void
    {
        if (participantId)
        {
            this._volunteerPlanningService.assignToShift(shiftId, participantId, false)
                .pipe(takeUntil(this._unsubscribeAll))
                .subscribe(() =>
                {
                    // Data will be automatically refreshed through NotificationService
                });
        }
    }

    /**
     * Open assignment dialog
     */
    openAssignmentDialog(shift: any, role: 'responsible' | 'helper', helperIndex?: number): void
    {
        const dialogRef = this._dialog.open(ShiftAssignmentsComponent, {
            width: '600px',
            data: {
                shift: shift,
                role: role,
                helperIndex: helperIndex
            }
        });

        dialogRef.afterClosed().subscribe(result =>
        {
            if (result)
            {
                // Refresh data after assignment
                this.loadShifts();
            }
        });
    }

    /**
     * Get participant name by ID
     */
    getParticipantName(participantId: string): string
    {
        const participant = this.availableParticipants.find(p => p.registrationId === participantId);
        return participant ? `${participant.firstName} ${participant.lastName}` : '';
    }

    /**
     * Get participant email by ID
     */
    getParticipantEmail(participantId: string): string
    {
        const participant = this.availableParticipants.find(p => p.registrationId === participantId);
        return participant?.email || '';
    }

    /**
     * Unassign participant
     */
    unassignParticipant(shiftId: string, participantId: string): void
    {
        this._volunteerPlanningService.unassignFromShift(shiftId, participantId)
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe(() =>
            {
                // Data will be automatically refreshed through NotificationService
            });
    }

    /**
     * Get helper assignments for a shift (up to maxHelpers)
     */
    getHelperAssignments(shift: ShiftDisplayItem): any[]
    {
        const helpers = shift.assignments?.filter(a => a.registrationId !== shift.responsibleRegistrationId) || [];
        const result = [];
        for (let i = 0; i < this.maxHelpers; i++)
        {
            result.push(helpers[i] || null);
        }
        return result;
    }

    /**
     * Get responsible assignment for a shift
     */
    getResponsibleAssignment(shift: ShiftDisplayItem): any
    {
        return shift.assignments?.find(a => a.registrationId === shift.responsibleRegistrationId) || null;
    }

    /**
     * Delete shift
     */
    deleteShift(shift: any): void
    {
        // Open the confirmation dialog
        const confirmation = this._fuseConfirmationService.open({
            title: 'Delete shift',
            message: 'Are you sure you want to delete this shift? This action cannot be undone!',
            actions: {
                confirm: {
                    label: 'Delete'
                }
            }
        });

        // Subscribe to the confirmation dialog closed action
        confirmation.afterClosed().subscribe((result) =>
        {
            // If the confirm button pressed...
            if (result === 'confirmed')
            {
                // Delete the shift
                this._volunteerPlanningService.deleteShift(shift.id)
                    .pipe(takeUntil(this._unsubscribeAll))
                    .subscribe(() =>
                    {
                        // Data will be automatically refreshed through NotificationService
                        // No need to manually update the shifts array
                    });
            }
        });
    }

    /**
     * Track by function for ngFor loops
     */
    trackByFn(index: number, item: any): any
    {
        return item.id || index;
    }

    /**
     * Load shifts data
     */
    loadShifts(): void
    {
        // Since we're using an observable pattern, we just need to trigger a refresh
        // The shifts$ observable will automatically update
        this.shifts$ = this._volunteerPlanningService.fetchShifts();
        this._changeDetectorRef.markForCheck();
    }

    private loadAvailableParticipants(): void
    {
        // For now, we'll load all participants
        // In a real implementation, you might want to load participants who are available for volunteer work
        this._volunteerPlanningService.getAvailableParticipants('')
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((participants) =>
            {
                this.availableParticipants = participants;
            });
    }
}
