import { NgModule } from '@angular/core';
import { MatLegacyButtonModule as MatButtonModule } from '@angular/material/legacy-button';
import { MatIconModule } from '@angular/material/icon';
import { MatLegacyMenuModule as MatMenuModule } from '@angular/material/legacy-menu';
import { LanguagesComponent } from 'app/layout/common/languages/languages.component';
import { SharedModule } from 'app/shared/shared.module';

@NgModule({
    declarations: [
        LanguagesComponent
    ],
    imports     : [
        MatButtonModule,
        MatIconModule,
        MatMenuModule,
        SharedModule
    ],
    exports     : [
        LanguagesComponent
    ]
})
export class LanguagesModule
{
}
