import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit, ViewEncapsulation } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { Subject, takeUntil, Observable } from 'rxjs';
import { FuseConfirmationService } from '@fuse/services/confirmation';
import { VolunteerPlanningService } from '../volunteer-planning.service';
import { ParticipantDisplayItem, ShiftDisplayItem, ShiftGroup, RegistrableDisplayItem, AvailableQuestionOptionMapping } from 'app/api/api';
import { NavigatorService } from '../../navigator.service';
import { RegistrablesService } from '../../pricing/registrables.service';
import { ShiftEditComponent } from '../shift-edit/shift-edit.component';

@Component({
    selector: 'app-shifts-overview',
    templateUrl: './shifts-overview.component.html',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ShiftsOverviewComponent implements OnInit, OnDestroy
{
    shiftGroups$: Observable<ShiftGroup[]>;
    currentShiftGroups: ShiftGroup[] = [];
    availableParticipants: ParticipantDisplayItem[] = [];
    candidates: ParticipantDisplayItem[] = [];
    searchString: string = '';
    showCandidates: boolean = false;
    currentAssignment: { shiftId: string; role: 'responsible' | 'helper'; helperIndex?: number; } | null = null;
    maxHelpers: number = 3; // Maximum number of helper columns to show
    maxHelpersNeeded: number = 3; // Dynamic maximum based on shifts data
    // Preference selection properties
    showPreferenceSelection: boolean = false;
    currentShiftForPreference: ShiftDisplayItem | null = null;
    preferenceSearchString: string = '';
    // Configuration properties
    allRegistrables: RegistrableDisplayItem[] = [];
    allQuestionOptions: AvailableQuestionOptionMapping[] = [];
    selectedRegistrableIds: string[] = [];
    configurationLoaded: boolean = false;
    configurationCollapsed: boolean = true;

    private _unsubscribeAll: Subject<any> = new Subject<any>();

    constructor(
        private _changeDetectorRef: ChangeDetectorRef,
        private _fuseConfirmationService: FuseConfirmationService,
        private _volunteerPlanningService: VolunteerPlanningService,
        public _navigatorService: NavigatorService,
        private _registrablesService: RegistrablesService,
        private _matDialog: MatDialog
    ) { }

    /**
     * Get filtered candidates based on search string
     */
    get filteredCandidates(): ParticipantDisplayItem[]
    {
        if (!this.searchString.trim())
        {
            return this.candidates;
        }

        const searchTerm = this.searchString.toLowerCase().trim();
        return this.candidates.filter(candidate =>
            candidate.firstName?.toLowerCase().includes(searchTerm) ||
            candidate.lastName?.toLowerCase().includes(searchTerm) ||
            candidate.email?.toLowerCase().includes(searchTerm)
        );
    }

    /**
     * Get filtered registrables based on search string
     */
    get filteredRegistrables(): RegistrableDisplayItem[]
    {
        if (!this.preferenceSearchString.trim())
        {
            return this.allRegistrables;
        }

        const searchTerm = this.preferenceSearchString.toLowerCase().trim();
        return this.allRegistrables.filter(registrable =>
            registrable.name?.toLowerCase().includes(searchTerm) ||
            registrable.nameSecondary?.toLowerCase().includes(searchTerm)
        );
    }

    /**
     * Get candidates who prefer this time
     */
    get candidatesWhoPreferTime(): ParticipantDisplayItem[]
    {
        const filtered = this.filteredCandidates.filter(candidate => candidate.prefersTime === true);
        return filtered;
    }

    /**
     * Get candidates who don't prefer this time
     */
    get candidatesWhoDoNotPreferTime(): ParticipantDisplayItem[]
    {
        const filtered = this.filteredCandidates.filter(candidate => candidate.prefersTime === false);
        return filtered;
    }

    ngOnInit(): void
    {
        // Subscribe to shiftGroups$ observable for reactive updates
        this.shiftGroups$ = this._volunteerPlanningService.shiftGroups$;

        // Subscribe to shift groups to keep currentShiftGroups updated
        this.shiftGroups$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((shiftGroups) =>
            {
                this.currentShiftGroups = shiftGroups || [];
                // Calculate max helpers needed across all shifts in all groups
                const allShifts = this.currentShiftGroups.flatMap(group => group.shifts || []);
                this.maxHelpersNeeded = Math.max(...allShifts.map(s => s.helpersNeeded || 0), 0);
                this._changeDetectorRef.markForCheck();
            });

        // Load configuration data
        this.loadConfiguration();

        // Trigger initial data fetch
        this._volunteerPlanningService.fetchShifts()
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe();
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
    getHelperColumns(shiftGroup: ShiftGroup): number[]
    {
        return Array.from({ length: Math.max(...shiftGroup.shifts.map(s => s.helpersNeeded || 0), 0) }, (_, i) => i + 1);
    }

    /**
     * Create new shift automatically using the last shift's end time
     */
    createShift(shiftGroup: ShiftGroup | null = null): void
    {
        const lastShift = shiftGroup?.shifts.reduce((latest, current) =>
            new Date(current.endTime || 0) > new Date(latest.endTime || 0) ? current : latest);

        const startTime = lastShift?.endTime ?? shiftGroup?.day ?? new Date();
        // Calculate end time (1 hour after start time by default)
        const endTime = new Date(startTime);
        endTime.setHours(endTime.getHours() + 1);

        // Call the service to create the shift
        this._volunteerPlanningService.createShift(
            shiftGroup.location,
            startTime,
            endTime);
    }



    /**
     * Edit shift
     */
    editShift(shift: ShiftDisplayItem): void
    {
        // Open the dialog
        const dialogRef = this._matDialog.open(ShiftEditComponent, {
            width: '640px',
            data: {
                shift: shift
            }
        });

        // Handle the result
        dialogRef.afterClosed()
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((result) =>
            {
                // Dialog closed, data will be automatically refreshed through NotificationService
                // No additional action needed
            });
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
     * Remove helper slot from shift
     */
    removeHelperSlot(shift: ShiftDisplayItem): void
    {
        this._volunteerPlanningService.removeHelperSlot(shift.id)
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe({
                next: () =>
                {
                    console.log('Helper slot removed successfully');
                },
                error: (error) =>
                {
                    console.error('Error removing helper slot:', error);
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
     * Show candidates for assignment
     */
    showCandidatesForAssignment(shift: any, role: 'responsible' | 'helper', helperIndex?: number): void
    {
        this.currentAssignment = {
            shiftId: shift.id,
            role: role,
            helperIndex: helperIndex
        };

        // Fetch available participants for this shift
        this._volunteerPlanningService.getAvailableParticipants(shift.id)
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe({
                next: (participants) =>
                {
                    this.candidates = participants;
                    this.showCandidates = true;
                    this._changeDetectorRef.markForCheck();
                },
                error: (error) =>
                {
                    console.error('Error fetching available participants:', error);
                }
            });
    }

    /**
     * Select candidate for assignment
     */
    selectCandidate(participant: ParticipantDisplayItem | null): void
    {
        if (!this.currentAssignment)
        {
            return;
        }

        if (participant === null)
        {
            // Unassign current participant
            const currentAssigned = this.getCurrentAssignedParticipant();
            if (currentAssigned)
            {
                this.unassignParticipant(this.currentAssignment.shiftId, currentAssigned.registrationId, this.currentAssignment.role === 'responsible');
            }
        }
        else
        {
            // Assign new participant
            if (this.currentAssignment.role === 'responsible')
            {
                this.assignResponsible(this.currentAssignment.shiftId, participant.registrationId);
            } else
            {
                this.assignHelper(this.currentAssignment.shiftId, participant.registrationId, this.currentAssignment.helperIndex || 0);
            }
        }

        this.closeCandidateSelection();
    }

    /**
     * Close candidate selection
     */
    closeCandidateSelection(): void
    {
        this.showCandidates = false;
        this.candidates = [];
        this.searchString = '';
        this.currentAssignment = null;
        this._changeDetectorRef.markForCheck();
    }

    /**
     * Unassign participant
     */
    unassignParticipant(shiftId: string, participantId: string, fromResponsible: boolean = false): void
    {
        this._volunteerPlanningService.unassignFromShift(shiftId, participantId, fromResponsible)
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe(() =>
            {
                // Data will be automatically refreshed through NotificationService
            });
    }

    /**
     * Get current assigned participant for the role being edited
     */
    getCurrentAssignedParticipant(): { registrationId: string; participant: string; email?: string; } | null
    {
        if (!this.currentAssignment || !this.currentShiftGroups)
        {
            return null;
        }

        // Find the shift across all groups
        let shift: ShiftDisplayItem | undefined;
        for (const group of this.currentShiftGroups)
        {
            shift = group.shifts?.find(s => s.id === this.currentAssignment!.shiftId);
            if (shift) break;
        }

        if (!shift)
        {
            return null;
        }

        if (this.currentAssignment.role === 'responsible')
        {
            if (shift.responsibleRegistrationId)
            {
                return {
                    registrationId: shift.responsibleRegistrationId,
                    participant: shift.participantResponsible || '',
                    email: shift.responsibleEmail
                };
            }
        }
        else if (this.currentAssignment.role === 'helper' && this.currentAssignment.helperIndex !== undefined)
        {
            // Get helper assignments (excluding responsible) to match what's displayed in the UI
            const helperAssignments = shift.assignments?.filter(a => a.registrationId !== shift.responsibleRegistrationId) || [];
            const assignment = helperAssignments[this.currentAssignment.helperIndex];
            if (assignment?.registrationId)
            {
                return {
                    registrationId: assignment.registrationId,
                    participant: assignment.participant || '',
                    email: assignment.email
                };
            }
        }

        return null;
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
     * Track by function for group ngFor loops
     */
    trackByGroupFn(index: number, group: ShiftGroup): any
    {
        return `${group.day}-${group.location}` || index;
    }

    /**
     * Load shifts data
     */
    loadShifts(): void
    {
        // Since we're using an observable pattern, we just need to trigger a refresh
        // The shiftGroups$ observable will automatically update
        this.shiftGroups$ = this._volunteerPlanningService.fetchShifts();
        this._changeDetectorRef.markForCheck();
    }

    /**
     * Update configuration when selections change
     */
    updateConfiguration(): void
    {
        this._volunteerPlanningService.updateVolunteerAdminConfiguration(
            this.selectedRegistrableIds
        )
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe(() =>
            {
                // Configuration updated successfully
                this._changeDetectorRef.markForCheck();
            });
    }

    /**
     * Toggle configuration panel visibility
     */
    toggleConfiguration(): void
    {
        this.configurationCollapsed = !this.configurationCollapsed;
        this._changeDetectorRef.markForCheck();
    }

    /**
     * Show preference selector overlay
     */
    showPreferenceSelector(shift: ShiftDisplayItem): void
    {
        this.currentShiftForPreference = shift;
        this.showPreferenceSelection = true;
        this._changeDetectorRef.markForCheck();
    }

    /**
     * Close preference selection overlay
     */
    closePreferenceSelection(): void
    {
        this.showPreferenceSelection = false;
        this.currentShiftForPreference = null;
        this.preferenceSearchString = '';
        this._changeDetectorRef.markForCheck();
    }

    /**
     * Select preference registrable
     */
    selectPreference(registrableId: string | null): void
    {
        if (this.currentShiftForPreference)
        {
            // Update shift
            this.currentShiftForPreference.shiftPreferenceRegistrableId = registrableId;
            this._volunteerPlanningService.updateShift(this.currentShiftForPreference.id, this.currentShiftForPreference)
                .pipe(takeUntil(this._unsubscribeAll))
                .subscribe({
                    next: (_) =>
                    {
                        // Data will be automatically refreshed through NotificationService
                        this.closePreferenceSelection();
                    }
                });
        }
        this.closePreferenceSelection();
    }

    /**
     * Load configuration data (registrables, question options, and current selections)
     */
    private loadConfiguration(): void
    {
        // Load registrables
        this._registrablesService.fetchRegistrables()
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((registrables) =>
            {
                this.allRegistrables = registrables;
                this._changeDetectorRef.markForCheck();
            });

        // Load current configuration
        this._volunteerPlanningService.getVolunteerAdminConfiguration()
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((config) =>
            {
                this.selectedRegistrableIds = config.registrableIds_Volunteer || [];
                this.configurationLoaded = true;
                this._changeDetectorRef.markForCheck();
            });
    }
}
