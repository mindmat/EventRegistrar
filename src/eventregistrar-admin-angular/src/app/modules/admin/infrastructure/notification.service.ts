import { Injectable, NgZone } from '@angular/core';
import { HttpTransportType, HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { filter, map, mergeMap, Observable, ReplaySubject, shareReplay, Subject, tap } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class NotificationService
{
    private serverEvents$ = new Subject<ReadModelUpdated>();
    private hubConnection: HubConnection;
    private connectionEstablished$ = new Subject<Boolean>();
    private subscription$ = new ReplaySubject<string>();
    private subscribedEventId: string;
    private zone = new NgZone({ enableLongStackTrace: false });
    private _isConnected$ = new Subject<boolean>();

    constructor()
    {
        this.hubConnection = new HubConnectionBuilder()
            .configureLogging(LogLevel.Debug)
            .withUrl(`https://localhost:5001/notifications`, {
                skipNegotiation: true,
                transport: HttpTransportType.WebSockets
            })
            .withAutomaticReconnect()
            .build();

        this.hubConnection.on('Process', (eventId: string, queryName: string, rowId: string) =>
        {
            const notification = {
                eventId,
                queryName,
                rowId
            } as ReadModelUpdated;
            console.log(notification);
            // this.serverEvents$.next(notification);
            this.zone.run(() =>
            {
                this.serverEvents$.next(notification);
            });
        });

        this.hubConnection.onreconnecting(() => this._isConnected$.next(false));
        this.hubConnection.onreconnected(() =>
        {
            this.subscription$.next(this.subscribedEventId);
            this._isConnected$.next(true);
        });

        this.initializeSubscriptions();
        this.startConnection();

        this.serverEvents$.subscribe(
            ntf => console.log(`notification ${ntf.queryName}`)
        );
    }

    public subscribe(queryName: string): Observable<ReadModelUpdated>
    {
        return this.serverEvents$.pipe(
            filter(ntf => ntf.queryName.toLowerCase() === queryName.toLowerCase())
        );
    }

    public switchToEvent(eventId: string): void
    {
        this.subscribedEventId = eventId;
        this.subscription$.next(eventId);
    }

    get isConnected$()
    {
        return this._isConnected$.asObservable();
    }

    get reconnected$(): Observable<typeof RECONNECTED>
    {
        return this._isConnected$.pipe(
            filter(connected => connected),
            map(_ => RECONNECTED),
        );
    }

    private startConnection()
    {
        this.hubConnection
            .start()
            .then(() =>
            {
                this.connectionEstablished$.next(true);
            })
            .catch(err =>
            {
                console.warn('Error while establishing connection: ' + err + ', retrying...');
                setTimeout(() => this.startConnection(), 5000);
            });
    }

    private initializeSubscriptions()
    {
        this.connectionEstablished$.pipe(
            filter(c => !!c),
            mergeMap(_ => this.subscription$),
        ).subscribe(name => this.hubConnection.invoke('SubscribeToEvent', name));
    }
}

export const RECONNECTED = Symbol('reconnected');

export class ReadModelUpdated 
{
    public queryName: string;
    public rowId: string;
    public eventId: string;
    public userId: string;
}