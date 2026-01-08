import { NgModule } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatRippleModule } from '@angular/material/core';
import { OverlayModule } from '@angular/cdk/overlay';
import { PortalModule } from '@angular/cdk/portal';
import { TranslateModule } from '@ngx-translate/core';
import { TagsPickerComponent } from './tags-picker/tags-picker.component';

@NgModule({
    declarations: [

    ],
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        MatIconModule,
        MatTooltipModule,
        MatRippleModule,
        OverlayModule,
        PortalModule,
        TranslateModule,
        TagsPickerComponent
    ],
    exports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        TagsPickerComponent
    ],
    providers: [
        DatePipe
    ]
})
export class SharedModule
{
}
