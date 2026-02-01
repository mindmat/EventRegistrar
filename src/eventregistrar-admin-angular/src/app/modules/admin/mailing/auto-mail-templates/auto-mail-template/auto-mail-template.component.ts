import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Api, AutoMailTemplateDisplayItem, MailType, PlaceholderDescription } from 'app/api/api';
import { EventService } from 'app/modules/admin/events/event.service';
import { Subject, takeUntil } from 'rxjs';
import { AutoMailTemplateService } from './auto-mail-template.service';
import { TranslateService } from '@ngx-translate/core';
import { HtmlMailEditorComponent, PlaceholderItem } from 'app/shared/html-mail-editor/html-mail-editor.component';

@Component({
  selector: 'app-auto-mail-template',
  templateUrl: './auto-mail-template.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AutoMailTemplateComponent implements OnInit
{
  @ViewChild('editor') editor: HtmlMailEditorComponent;

  private unsubscribeAll: Subject<any> = new Subject<any>();
  placeholders: PlaceholderItem[] = [];

  templateForm = this.fb.group<AutoMailTemplateDisplayItem>({
    id: '',
    subject: '',
    contentHtml: '',
    addIcs: false,
    warnings: null
  });

  constructor(private service: AutoMailTemplateService,
    private fb: FormBuilder,
    private api: Api,
    private eventService: EventService,
    private changeDetectorRef: ChangeDetectorRef,
    private translateService: TranslateService) { }

  ngOnInit(): void
  {
    this.service.template$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((template: AutoMailTemplateDisplayItem) =>
      {
        this.templateForm.patchValue(template);
        this.updatePlaceholders(template.type);

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });
  }

  updatePlaceholders(type: MailType)
  {
    this.api.autoMailPlaceholder_Query({ mailType: type })
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((placeholderDescriptions: PlaceholderDescription[]) =>
      {
        this.placeholders = placeholderDescriptions.map(p => ({
          placeholder: p.placeholder,
          description: p.description
        }));
        this.changeDetectorRef.markForCheck();
      });
  }

  openPreview()
  {
    var url = `${this.eventService.selected.acronym}/mail-template-preview/${this.templateForm.value.id}`;
    window.open(url, '_blank', 'location=yes,height=900,width=700,scrollbars=yes,status=yes'); // Open new window
  }

  save()
  {
    this.api.updateAutoMailTemplate_Command({
      eventId: this.eventService.selectedId,
      templateId: this.templateForm.value.id,
      subject: this.templateForm.value.subject,
      addIcs: this.templateForm.value.addIcs,
      contentHtml: this.templateForm.value.contentHtml
    })
      .subscribe();
  }
}
