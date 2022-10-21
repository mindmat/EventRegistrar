import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { RegistrableDisplayItem, RegistrationFormItem } from 'app/api/api';
import { Subject, takeUntil } from 'rxjs';
import { FormsService } from './forms.service';
import { RegistrablesService } from './registrables.service';

@Component({
  selector: 'app-form-mapping',
  templateUrl: './form-mapping.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FormMappingComponent implements OnInit
{
  private unsubscribeAll: Subject<any> = new Subject<any>();
  forms: RegistrationFormItem[];
  allRegistrables: RegistrableDisplayItem[];
  constructor(private formsService: FormsService,
    private registrablesService: RegistrablesService,
    private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    this.formsService.forms$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((forms: RegistrationFormItem[]) =>
      {
        this.forms = forms;

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });

    this.registrablesService.registrables$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((registrables: RegistrableDisplayItem[]) =>
      {
        this.allRegistrables = registrables;

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });
  }

  importFormUpdate(form: RegistrationFormItem)
  {
    this.formsService.importForm(form.externalIdentifier);
  }
}
