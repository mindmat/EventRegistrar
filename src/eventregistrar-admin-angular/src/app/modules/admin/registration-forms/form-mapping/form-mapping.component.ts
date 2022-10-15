import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { RegistrationFormItem } from 'app/api/api';
import { Subject, takeUntil } from 'rxjs';
import { FormsService } from './forms.service';

@Component({
  selector: 'app-form-mapping',
  templateUrl: './form-mapping.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FormMappingComponent implements OnInit
{
  private unsubscribeAll: Subject<any> = new Subject<any>();
  forms: RegistrationFormItem[];
  constructor(private formsService: FormsService,
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
  }

}
