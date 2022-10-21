import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { AvailableQuestionMapping, AvailableQuestionOptionMapping, RegistrationFormItem } from 'app/api/api';
import { Subject, takeUntil } from 'rxjs';
import { FormsService } from './forms.service';
import { QuestionMappingService } from './question-mapping.service';
import { QuestionOptionMappingService } from './question-option-mapping.service';

@Component({
  selector: 'app-form-mapping',
  templateUrl: './form-mapping.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FormMappingComponent implements OnInit
{
  private unsubscribeAll: Subject<any> = new Subject<any>();
  forms: RegistrationFormItem[];
  allOptionMappings: AvailableQuestionOptionMapping[];
  allQuestionMappings: AvailableQuestionMapping[];

  constructor(private formsService: FormsService,
    private questionMappingService: QuestionMappingService,
    private questionOptionMappingService: QuestionOptionMappingService,
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

    this.questionMappingService.questionMappings
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((mappings: AvailableQuestionMapping[]) =>
      {
        this.allQuestionMappings = mappings;

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });

    this.questionOptionMappingService.questionOptionMappings
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((mappings: AvailableQuestionOptionMapping[]) =>
      {
        this.allOptionMappings = mappings;

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });
  }

  importFormUpdate(form: RegistrationFormItem)
  {
    this.formsService.importForm(form.externalIdentifier);
  }
}
