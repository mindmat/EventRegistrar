import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject, OnDestroy, OnInit, ViewEncapsulation } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Subject, takeUntil } from 'rxjs';
import { FuseConfirmationService } from '@fuse/services/confirmation';
import { VolunteerPlanningService } from '../volunteer-planning.service';

@Component({
    selector: 'app-shift-assignments',
    templateUrl: './shift-assignments.component.html',
    styleUrls: ['./shift-assignments.component.scss'],
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ShiftAssignmentsComponent implements OnInit, OnDestroy
{
    shift: any = null;
    availableParticipants: any[] = [];
    assignedParticipants: any[] = [];
    isLoading: boolean = false;
    role: 'responsible' | 'helper';
    helperIndex?: number;

    private _unsubscribeAll: Subject<any> = new Subject<any>();

    constructor(
        private _changeDetectorRef: ChangeDetectorRef,
        private _dialogRef: MatDialogRef<ShiftAssignmentsComponent>,
        private _fuseConfirmationService: FuseConfirmationService,
        private _volunteerPlanningService: VolunteerPlanningService,
        @Inject(MAT_DIALOG_DATA) public data: { shift: any, role: 'responsible' | 'helper', helperIndex?: number; }
    ) 
    {
        this.shift = data.shift;
        this.role = data.role;
        this.helperIndex = data.helperIndex;
    }

    ngOnInit(): void
    {
        // Load available participants for this shift
        this._volunteerPlanningService.getAvailableParticipants(this.shift.id)
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((participants) =>
            {
                this.availableParticipants = participants;
                this._changeDetectorRef.markForCheck();
            });
    }

    ngOnDestroy(): void
    {
        // Unsubscribe from all subscriptions
        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }

    /**
     * Assign participant to shift
     */
    assignParticipant(participant: any, asResponsible: boolean): void
    {
        this.isLoading = true;

        this._volunteerPlanningService.assignToShift(this.shift.id, participant.id, asResponsible)
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe({
                next: (result) =>
                {
                    // Close dialog with success result
                    this._dialogRef.close(true);
                },
                error: (error) =>
                {
                    this.isLoading = false;
                    this._changeDetectorRef.markForCheck();
                }
            });
    }

    /**
     * Unassign participant from shift
     */
    unassignParticipant(participantId: string): void
    {
        // Open the confirmation dialog
        const confirmation = this._fuseConfirmationService.open({
            title: 'Unassign participant',
            message: 'Are you sure you want to unassign this participant from the shift?',
            actions: {
                confirm: {
                    label: 'Unassign'
                }
            }
        });

        // Subscribe to the confirmation dialog closed action
        confirmation.afterClosed().subscribe((result) =>
        {
            // If the confirm button pressed...
            if (result === 'confirmed')
            {
                this.isLoading = true;

                this._volunteerPlanningService.unassignFromShift(this.shift.id, participantId)
                    .pipe(takeUntil(this._unsubscribeAll))
                    .subscribe({
                        next: (_) =>
                        {
                            // Data will be automatically refreshed through NotificationService
                            // But we still refresh local data for immediate feedback
                            this.refreshAssignments();
                        },
                        error: (error) =>
                        {
                            this.isLoading = false;
                            this._changeDetectorRef.markForCheck();
                        }
                    });
            }
        });
    }

    /**
     * Close dialog
     */
    closeDialog(): void
    {
        this._dialogRef.close();
    }

    /**
     * Track by function for ngFor loops
     */
    trackByFn(index: number, item: any): any
    {
        return item.id || index;
    }

    /**
     * Check if participant has preferred time that matches shift
     */
    hasPreferredTime(participant: any): boolean
    {
        if (!participant.preferredTimes || !this.shift)
        {
            return false;
        }
        // Simple check - in real implementation you'd compare time ranges
        return participant.preferredTimes.some((time: any) =>
            this.isTimeOverlapping(time.startTime, time.endTime, this.shift.startDate, this.shift.endDate)
        );
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Private methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Refresh assignments
     */
    private refreshAssignments(): void
    {
        // Reload the resolver data
        this._volunteerPlanningService.getAvailableParticipants(this.shift.id)
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe({
                next: (participants) =>
                {
                    this.availableParticipants = participants;
                    this.isLoading = false;
                    this._changeDetectorRef.markForCheck();
                },
                error: (error) =>
                {
                    this.isLoading = false;
                    this._changeDetectorRef.markForCheck();
                }
            });
    }

    private isTimeOverlapping(start1: Date, end1: Date, start2: Date, end2: Date): boolean
    {
        return start1 <= end2 && end1 >= start2;
    }
}