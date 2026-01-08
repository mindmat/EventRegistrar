import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { TranslateModule } from '@ngx-translate/core';
import { BankAccountConfiguration } from 'app/api/api';
import { Subject, takeUntil } from 'rxjs';
import { AccountConfigService } from './account-config.service';

@Component({
  selector: 'app-account-config',
  templateUrl: './account-config.component.html',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule
  ]
})
export class AccountConfigComponent implements OnInit
{
  public configForm: FormGroup;
  public submittable: boolean;
  private unsubscribeAll: Subject<any> = new Subject<any>();

  constructor(private bankAccountService: AccountConfigService,
    private fb: FormBuilder,
    private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    this.bankAccountService.bankAccount$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((cfg: BankAccountConfiguration) =>
      {
        this.configForm = this.fb.group(
          {
            iban: cfg.iban,
            accountHolderName: cfg.accountHolderName,
            accountHolderStreet: cfg.accountHolderStreet,
            accountHolderHouseNo: cfg.accountHolderHouseNo,
            accountHolderPostalCode: cfg.accountHolderPostalCode,
            accountHolderTown: cfg.accountHolderTown,
            accountHolderCountryCode: cfg.accountHolderCountryCode,
          });
        this.configForm.statusChanges.subscribe((_) =>
        {
          this.submittable = this.configForm.valid && this.configForm.dirty;
          this.changeDetectorRef.markForCheck();
        });

        this.changeDetectorRef.markForCheck();
      });
  }

  save(): void
  {
    this.bankAccountService.save(this.configForm.value as BankAccountConfiguration);
  }
}
