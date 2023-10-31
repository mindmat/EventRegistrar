import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { AvailableQuestionMapping, AvailableQuestionOptionMapping, MappingType, RegistrationFormItem } from 'app/api/api';
import { BehaviorSubject, Subject, takeUntil } from 'rxjs';
import { FormsService } from './forms.service';
import { QuestionMappingService } from './question-mapping.service';
import { QuestionOptionMappingService } from './question-option-mapping.service';
import { v4 as createUuid } from 'uuid';

@Component({
  selector: 'app-form-mapping',
  templateUrl: './form-mapping.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FormMappingComponent implements OnInit
{
  forms: RegistrationFormItem[];
  allOptionMappings: AvailableQuestionOptionMapping[];
  allQuestionMappings: AvailableQuestionMapping[];
  // mappingDirection$: BehaviorSubject<MappingDirection> = new BehaviorSubject<MappingDirection>(MappingDirection.FormToTrack);
  MappingDirection = MappingDirection;
  availableTracks: AvailableQuestionOptionMapping[];
  allQuestionOptions: QuestionOption[];
  private unsubscribeAll: Subject<any> = new Subject<any>();

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
        this.allQuestionOptions = forms.flatMap(frm => frm.sections.flatMap(fsc => fsc.questions.flatMap(fqs => fqs.options.map(fop => (fop as QuestionOption)))));

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
        this.availableTracks = mappings.filter(map => map.type === MappingType.SingleRegistrable || map.type === MappingType.PartnerRegistrable);

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });
  }

  // setMappingDirection(mappingDirection: MappingDirection)
  // {
  //   this.mappingDirection$.next(mappingDirection);
  // }

  importFormUpdate(form: RegistrationFormItem)
  {
    this.formsService.importForm(form.externalIdentifier);
  }

  addMultiMapping(form: RegistrationFormItem)
  {
    let sortKey = form.multiMappings.length === 0
      ? 1
      : Math.max(...form.multiMappings.map(mqm => mqm.sortKey)) + 1;
    console.log(sortKey);
    form.multiMappings.push({
      id: createUuid(),
      questionOptionIds: [],
      registrableCombinedIds: [],
      sortKey: sortKey ?? 1
    });
  }

  saveMappings(form: RegistrationFormItem)
  {
    this.formsService.saveMappings(form.registrationFormId, form.sections, form.multiMappings);
  }
}

enum MappingDirection
{
  FormToTrack = 1,
  TrackToForm = 2
}

interface QuestionOption
{
  id: string;
  answer: string;
}
