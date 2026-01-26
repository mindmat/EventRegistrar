import { Injectable } from '@angular/core';
import { Observable, Subscription } from 'rxjs';
import { Api, CreateShiftCommand, UpdateShiftCommand, DeleteShiftCommand, AssignToShiftCommand, UnassignFromShiftCommand, AddHelperSlotCommand, RemoveHelperSlotCommand, AvailableParticipantsQuery, ShiftsOverviewQuery, ShiftGroup, ShiftDisplayItem, ParticipantDisplayItem, VolunteerAdminConfigurationQuery, VolunteerAdminConfigurationDto, UpdateVolunteerAdminConfigurationCommand, ConfirmShiftAssignmentCommand, UnconfirmShiftAssignmentCommand } from 'app/api/api';
import { FetchService } from '../infrastructure/fetchService';
import { NotificationService } from '../infrastructure/notification.service';
import { EventService } from '../events/event.service';
import { v4 as createUuid } from 'uuid';

@Injectable({
    providedIn: 'root'
})
export class VolunteerPlanningService extends FetchService<ShiftGroup[]>
{
    constructor(
        private api: Api,
        private eventService: EventService,
        notificationService: NotificationService)
    {
        super('ShiftsOverviewQuery', notificationService);
    }

    /**
     * Get all shift groups with automatic updates via NotificationService
     */
    get shiftGroups$(): Observable<ShiftGroup[]>
    {
        return this.result$;
    }

    /**
     * Fetch shift groups from API
     */
    fetchShifts(): Observable<ShiftGroup[]>
    {
        const query: ShiftsOverviewQuery = {
            eventId: this.eventService.selectedId
        };
        return this.fetchItems(this.api.shiftsOverview_Query(query), null, this.eventService.selectedId);
    }

    /**
     * Get shift by ID - Note: You may need to implement a specific query for single shift
     */
    getShift(id: string): Observable<any>
    {
        // This might need a specific query implementation in the API
        // For now, get all shifts and filter client-side
        return this.fetchShifts();
    }

    /**
     * Create shift
     */
    createShift(location: string, startTime: Date, endTime: Date): void
    {
        this.api.createShift_Command({
            eventId: this.eventService.selectedId,
            shiftId: createUuid(),
            location,
            startTime,
            endTime,
        }).subscribe();
    }

    /**
     * Update shift
     */
    updateShift(shiftId: string, shiftData: ShiftDisplayItem): Observable<void>
    {
        const command: UpdateShiftCommand = {
            eventId: this.eventService.selectedId,
            shiftId: shiftId,
            name: shiftData.name,
            registrableId_ShiftPreference: shiftData.shiftPreferenceRegistrableId,
            description: shiftData.description,
            location: shiftData.location,
            startTime: shiftData.startTime,
            endTime: shiftData.endTime
        };
        return this.api.updateShift_Command(command);
    }

    /**
     * Add helper slot to shift
     */
    addHelperSlot(shiftId: string): Observable<void>
    {
        const command: AddHelperSlotCommand = {
            eventId: this.eventService.selectedId,
            shiftId: shiftId
        };
        return this.api.addHelperSlot_Command(command);
    }

    /**
     * Remove helper slot from shift
     */
    removeHelperSlot(shiftId: string): Observable<void>
    {
        const command: RemoveHelperSlotCommand = {
            eventId: this.eventService.selectedId,
            shiftId: shiftId
        };
        return this.api.removeHelperSlot_Command(command);
    }

    /**
     * Delete shift
     */
    deleteShift(shiftId: string): Observable<void>
    {
        const command: DeleteShiftCommand = {
            eventId: this.eventService.selectedId,
            shiftId
        };
        return this.api.deleteShift_Command(command);
    }

    /**
     * Get available participants for shift
     */
    getAvailableParticipants(shiftId: string): Observable<ParticipantDisplayItem[]>
    {
        const query: AvailableParticipantsQuery = {
            eventId: this.eventService.selectedId,
            shiftId: shiftId
        };
        return this.api.availableParticipants_Query(query);
    }

    /**
     * Assign participant to shift
     */
    assignToShift(shiftId: string, participantId: string, asResponsible: boolean): Observable<void>
    {
        const command: AssignToShiftCommand = {
            eventId: this.eventService.selectedId,
            shiftId: shiftId,
            registrationId: participantId,
            asResponsible
        };
        return this.api.assignToShift_Command(command);
    }

    /**
     * Unassign participant from shift
     */
    unassignFromShift(shiftId: string, participantId: string, fromResponsible: boolean = false): Observable<void>
    {
        const command: UnassignFromShiftCommand = {
            eventId: this.eventService.selectedId,
            shiftId: shiftId,
            registrationId: participantId,
            fromResponsible: fromResponsible
        };
        return this.api.unassignFromShift_Command(command);
    }

    /**
     * Get volunteer admin configuration
     */
    getVolunteerAdminConfiguration(): Observable<VolunteerAdminConfigurationDto>
    {
        const query: VolunteerAdminConfigurationQuery = {
            eventId: this.eventService.selectedId
        };
        return this.api.volunteerAdminConfiguration_Query(query);
    }

    /**
     * Update volunteer admin configuration
     */
    updateVolunteerAdminConfiguration(registrableIds: string[]): Observable<void>
    {
        const command: UpdateVolunteerAdminConfigurationCommand = {
            eventId: this.eventService.selectedId,
            registrableIds_Volunteer: registrableIds,
        };
        return this.api.updateVolunteerAdminConfiguration_Command(command);
    }

    /**
     * Confirm shift assignment
     */
    confirmShiftAssignment(shiftAssignmentId: string): Observable<void>
    {
        const command: ConfirmShiftAssignmentCommand = {
            eventId: this.eventService.selectedId,
            shiftAssignmentId: shiftAssignmentId
        };
        return this.api.confirmShiftAssignment_Command(command);
    }

    /**
     * Unconfirm shift assignment
     */
    unconfirmShiftAssignment(shiftAssignmentId: string): Observable<void>
    {
        const command: UnconfirmShiftAssignmentCommand = {
            eventId: this.eventService.selectedId,
            shiftAssignmentId: shiftAssignmentId
        };
        return this.api.unconfirmShiftAssignment_Command(command);
    }

    public recalculateReadModel(): Subscription
    {
        return this.api.updateReadModel_Command({ eventId: this.eventService.selectedId, queryName: 'ShiftsOverviewQuery' })
            .subscribe();
    }
}
