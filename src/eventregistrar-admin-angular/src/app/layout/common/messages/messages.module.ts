import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { OverlayModule } from '@angular/cdk/overlay';
import { PortalModule } from '@angular/cdk/portal';
import { MatLegacyButtonModule as MatButtonModule } from '@angular/material/legacy-button';
import { MatIconModule } from '@angular/material/icon';
import { MatLegacyTooltipModule as MatTooltipModule } from '@angular/material/legacy-tooltip';
import { MessagesComponent } from 'app/layout/common/messages/messages.component';
import { SharedModule } from 'app/shared/shared.module';

@NgModule({
    declarations: [
        MessagesComponent
    ],
    imports     : [
        RouterModule,
        OverlayModule,
        PortalModule,
        MatButtonModule,
        MatIconModule,
        MatTooltipModule,
        SharedModule
    ],
    exports     : [
        MessagesComponent
    ]
})
export class MessagesModule
{
}
