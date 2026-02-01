import { NgModule } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatRippleModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { OverlayModule } from '@angular/cdk/overlay';
import { PortalModule } from '@angular/cdk/portal';
import { TranslateModule, TranslatePipe } from '@ngx-translate/core';
import { TagsPickerComponent } from './tags-picker/tags-picker.component';
import { HtmlMailEditorComponent } from './html-mail-editor/html-mail-editor.component';

@NgModule({
    declarations: [
        TagsPickerComponent,
        HtmlMailEditorComponent
    ],
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        MatIconModule,
        MatTooltipModule,
        MatRippleModule,
        MatButtonModule,
        MatMenuModule,
        OverlayModule,
        PortalModule,
        TranslateModule
    ],
    exports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        TagsPickerComponent,
        HtmlMailEditorComponent
    ],
    providers: [
        DatePipe,
        TranslatePipe
    ]
})
export class SharedModule
{
}
