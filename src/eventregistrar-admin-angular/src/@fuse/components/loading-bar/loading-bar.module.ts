import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatLegacyProgressBarModule as MatProgressBarModule } from '@angular/material/legacy-progress-bar';
import { FuseLoadingBarComponent } from '@fuse/components/loading-bar/loading-bar.component';

@NgModule({
    declarations: [
        FuseLoadingBarComponent
    ],
    imports     : [
        CommonModule,
        MatProgressBarModule
    ],
    exports     : [
        FuseLoadingBarComponent
    ]
})
export class FuseLoadingBarModule
{
}
