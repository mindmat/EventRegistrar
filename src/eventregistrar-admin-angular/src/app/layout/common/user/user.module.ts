import { NgModule } from '@angular/core';
import { MatLegacyButtonModule as MatButtonModule } from '@angular/material/legacy-button';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatLegacyMenuModule as MatMenuModule } from '@angular/material/legacy-menu';
import { UserComponent } from 'app/layout/common/user/user.component';
import { SharedModule } from 'app/shared/shared.module';

@NgModule({
    declarations: [
        UserComponent
    ],
    imports     : [
        MatButtonModule,
        MatDividerModule,
        MatIconModule,
        MatMenuModule,
        SharedModule
    ],
    exports     : [
        UserComponent
    ]
})
export class UserModule
{
}
