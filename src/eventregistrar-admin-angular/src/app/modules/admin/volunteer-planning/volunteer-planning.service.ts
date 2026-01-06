import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Api, CreateShiftCommand, UpdateShiftCommand, DeleteShiftCommand, AssignToShiftCommand, UnassignFromShiftCommand, AddHelperSlotCommand, RemoveHelperSlotCommand, AvailableParticipantsQuery, ShiftsOverviewQuery, ShiftDisplayItem, ParticipantDisplayItem, VolunteerAdminConfigurationQuery, VolunteerAdminConfigurationDto, UpdateVolunteerAdminConfigurationCommand } from 'app/api/api';
import { FetchService } from '../infrastructure/fetchService';
import { NotificationService } from '../infrastructure/notification.service';
import { EventService } from '../events/event.service';
import { v4 as createUuid } from 'uuid';

@Injectable({
    providedIn: 'root'
})
export class VolunteerPlanningService extends FetchService<ShiftDisplayItem[]>
{
    constructor(
        private _api: Api,
        private _eventService: EventService,
        notificationService: NotificationService)
    {
        super('ShiftsOverviewQuery', notificationService);
    }

    /**
     * Get all shifts with automatic updates via NotificationService
     */
    get shifts$(): Observable<ShiftDisplayItem[]>
    {
        return this.result$;
    }

    /**
     * Fetch shifts from API
     */
    fetchShifts(): Observable<ShiftDisplayItem[]>
    {
        const query: ShiftsOverviewQuery = {
            eventId: this._eventService.selectedId
        };
        return this.fetchItems(this._api.shiftsOverview_Query(query), null, this._eventService.selectedId);
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
        this._api.createShift_Command({
            eventId: this._eventService.selectedId,
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
            eventId: this._eventService.selectedId,
            shiftId: shiftId,
            name: shiftData.name,
            registrableId_ShiftPreference: shiftData.shiftPreferenceRegistrableId,
            description: shiftData.description,
            location: shiftData.location,
            startTime: shiftData.startTime,
            endTime: shiftData.endTime
        };
        return this._api.updateShift_Command(command);
    }

    /**
     * Add helper slot to shift
     */
    addHelperSlot(shiftId: string): Observable<void>
    {
        const command: AddHelperSlotCommand = {
            eventId: this._eventService.selectedId,
            shiftId: shiftId
        };
        return this._api.addHelperSlot_Command(command);
    }

    /**
     * Remove helper slot from shift
     */
    removeHelperSlot(shiftId: string): Observable<void>
    {
        const command: RemoveHelperSlotCommand = {
            eventId: this._eventService.selectedId,
            shiftId: shiftId
        };
        return this._api.removeHelperSlot_Command(command);
    }

    /**
     * Delete shift
     */
    deleteShift(shiftId: string): Observable<void>
    {
        const command: DeleteShiftCommand = {
            eventId: this._eventService.selectedId,
            shiftId
        };
        return this._api.deleteShift_Command(command);
    }

    /**
     * Get available participants for shift
     */
    getAvailableParticipants(shiftId: string): Observable<ParticipantDisplayItem[]>
    {
        const query: AvailableParticipantsQuery = {
            eventId: this._eventService.selectedId,
            shiftId: shiftId
        };
        return this._api.availableParticipants_Query(query);
    }

    /**
     * Assign participant to shift
     */
    assignToShift(shiftId: string, participantId: string, asResponsible: boolean): Observable<void>
    {
        const command: AssignToShiftCommand = {
            eventId: this._eventService.selectedId,
            shiftId: shiftId,
            registrationId: participantId,
            asResponsible
        };
        return this._api.assignToShift_Command(command);
    }

    /**
     * Unassign participant from shift
     */
    unassignFromShift(shiftId: string, participantId: string): Observable<void>
    {
        const command: UnassignFromShiftCommand = {
            eventId: this._eventService.selectedId,
            shiftId: shiftId,
            registrationId: participantId
        };
        return this._api.unassignFromShift_Command(command);
    }

    /**
     * Get volunteer admin configuration
     */
    getVolunteerAdminConfiguration(): Observable<VolunteerAdminConfigurationDto>
    {
        const query: VolunteerAdminConfigurationQuery = {
            eventId: this._eventService.selectedId
        };
        return this._api.volunteerAdminConfiguration_Query(query);
    }

    /**
     * Update volunteer admin configuration
     */
    updateVolunteerAdminConfiguration(registrableIds: string[]): Observable<void>
    {
        const command: UpdateVolunteerAdminConfigurationCommand = {
            eventId: this._eventService.selectedId,
            registrableIds_Volunteer: registrableIds,
        };
        return this._api.updateVolunteerAdminConfiguration_Command(command);
    }
}
