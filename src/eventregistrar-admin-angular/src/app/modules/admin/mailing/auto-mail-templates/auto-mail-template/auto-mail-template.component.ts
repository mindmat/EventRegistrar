import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup } from '@angular/forms';
import { Api, AutoMailTemplateDisplayItem } from 'app/api/api';
import { EventService } from 'app/modules/admin/events/event.service';
import { Observable, Subject, takeUntil } from 'rxjs';
import { AutoMailTemplateService } from './auto-mail-template.service';

@Component({
  selector: 'app-auto-mail-template',
  templateUrl: './auto-mail-template.component.html'
})
export class AutoMailTemplateComponent implements OnInit
{
  private unsubscribeAll: Subject<any> = new Subject<any>();
  template: AutoMailTemplateDisplayItem;
  templateForm = this.fb.group<AutoMailTemplateDisplayItem>({
    id: '',
    subject: '',
    contentHtml: ''
  });

  constructor(private service: AutoMailTemplateService,
    private fb: FormBuilder,
    private api: Api,
    private eventService: EventService,
    private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    this.service.template$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((template: AutoMailTemplateDisplayItem) =>
      {
        this.template = template;
        this.templateForm.patchValue(template);

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });
  }

  save()
  {
    this.api.updateAutoMailTemplate_Command({
      eventId: this.eventService.selectedId,
      templateId: this.templateForm.value.id,
      subject: this.templateForm.value.subject,
      contentHtml: this.templateForm.value.contentHtml,
    })
      .subscribe(x => console.log(x));
  }
}
