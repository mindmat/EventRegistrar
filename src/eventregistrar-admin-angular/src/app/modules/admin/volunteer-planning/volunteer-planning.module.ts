import { NgModule } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';

// Material Design imports - all required modules
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatOptionModule, MatRippleModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSortModule } from '@angular/material/sort';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMomentDateModule } from '@angular/material-moment-adapter';

import { RouterModule } from '@angular/router';
import { FuseConfirmationModule } from '@fuse/services/confirmation';
import { SharedModule } from 'app/shared/shared.module';
import { TranslateModule } from '@ngx-translate/core';

import { ShiftsOverviewComponent } from './shifts-overview/shifts-overview.component';
import { ShiftEditComponent } from './shift-edit/shift-edit.component';

@NgModule({
    declarations: [
        ShiftsOverviewComponent,
        ShiftEditComponent
    ],
    imports: [
        CommonModule,
        ReactiveFormsModule,
        FormsModule,
        RouterModule,
        MatButtonModule,
        MatDatepickerModule,
        MatDialogModule,
        MatFormFieldModule,
        MatIconModule,
        MatInputModule,
        MatMomentDateModule,
        MatOptionModule,
        MatProgressSpinnerModule,
        MatRippleModule,
        MatSelectModule,
        MatSortModule,
        MatTooltipModule,
        FuseConfirmationModule,
        SharedModule,
        TranslateModule
    ]
})
export class VolunteerPlanningModule { }
