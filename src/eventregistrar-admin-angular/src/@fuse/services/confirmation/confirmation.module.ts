import { NgModule } from '@angular/core';
import { MatLegacyButtonModule as MatButtonModule } from '@angular/material/legacy-button';
import { MatLegacyDialogModule as MatDialogModule } from '@angular/material/legacy-dialog';
import { MatIconModule } from '@angular/material/icon';
import { FuseConfirmationService } from '@fuse/services/confirmation/confirmation.service';
import { FuseConfirmationDialogComponent } from '@fuse/services/confirmation/dialog/dialog.component';
import { CommonModule } from '@angular/common';

@NgModule({
    declarations: [
        FuseConfirmationDialogComponent
    ],
    imports     : [
        MatButtonModule,
        MatDialogModule,
        MatIconModule,
        CommonModule
    ],
    providers   : [
        FuseConfirmationService
    ]
})
export class FuseConfirmationModule
{
    /**
     * Constructor
     */
    constructor(private _fuseConfirmationService: FuseConfirmationService)
    {
    }
}
