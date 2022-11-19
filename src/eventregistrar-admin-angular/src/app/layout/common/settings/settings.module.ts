import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatLegacyTooltipModule as MatTooltipModule } from '@angular/material/legacy-tooltip';
import { FuseDrawerModule } from '@fuse/components/drawer';
import { SettingsComponent } from 'app/layout/common/settings/settings.component';
import { MatLegacyButtonModule as MatButtonModule } from '@angular/material/legacy-button';

@NgModule({
    declarations: [
        SettingsComponent
    ],
    imports     : [
        CommonModule,
        RouterModule,
        MatIconModule,
        MatTooltipModule,
        FuseDrawerModule,
        MatButtonModule
    ],
    exports     : [
        SettingsComponent
    ]
})
export class SettingsModule
{
}
