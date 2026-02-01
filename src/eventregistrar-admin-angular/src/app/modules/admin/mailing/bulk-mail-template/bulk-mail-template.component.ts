import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Api, BulkMailTemplateDisplayItem, GeneratedBulkMails, MailingAudience, PlaceholderDescription, PossibleAudience } from 'app/api/api';
import { Subject, takeUntil } from 'rxjs';
import { EventService } from '../../events/event.service';
import { BulkMailTemplateService } from './bulk-mail-template.service';
import { RegistrablesService } from '../../pricing/registrables.service';
import { GeneratedBulkMailsService } from './generated-bulk-mails.service';
import { HtmlMailEditorComponent, PlaceholderItem } from 'app/shared/html-mail-editor/html-mail-editor.component';

@Component({
  selector: 'app-bulk-mail-template',
  templateUrl: './bulk-mail-template.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BulkMailTemplateComponent implements OnInit
{
  @ViewChild('editor') editor: HtmlMailEditorComponent;

  possibleAudiences: PossibleAudience[];
  selectedAudiences: MailingAudience[] | null;
  registrableIds: string[] | null;
  mailsProgress: GeneratedBulkMails;
  placeholders: PlaceholderItem[] = [];

  templateForm = this.fb.group({
    id: '',
    senderName: '',
    senderMail: '',
    subject: '',
    contentHtml: '',
    addIcs: false,
  });

  private unsubscribeAll: Subject<any> = new Subject<any>();
  private bulkMailKey: string;

  constructor(private service: BulkMailTemplateService,
    private fb: FormBuilder,
    private api: Api,
    private eventService: EventService,
    public registrablesService: RegistrablesService,
    private generatedBulkMailsService: GeneratedBulkMailsService,
    private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    this.service.template$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((template: BulkMailTemplateDisplayItem) =>
      {
        this.templateForm.patchValue(template);
        this.selectedAudiences = template.audiences;
        this.registrableIds = template.registrableIds ?? [];
        if (this.bulkMailKey !== template.bulkMailKey)
        {
          this.bulkMailKey = template.bulkMailKey;
          this.generatedBulkMailsService.fetchMailCount(this.bulkMailKey).subscribe();
        }

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });

    this.service.getAvailablePlaceholders()
      .subscribe((placeholderDescriptions: PlaceholderDescription[]) =>
      {
        this.placeholders = placeholderDescriptions.map(p => ({
          placeholder: p.placeholder,
          description: p.description
        }));
        this.changeDetectorRef.markForCheck();
      });

    this.service.getAvailableAudiences()
      .subscribe(audiences => this.possibleAudiences = audiences);

    this.generatedBulkMailsService.generated$.subscribe((result) =>
    {
      this.mailsProgress = result;
      this.changeDetectorRef.markForCheck();
    });
  }

  openPreview(): void
  {
    const url = `${this.eventService.selected.acronym}/mail-template-preview/${this.templateForm.value.id}`;
    window.open(url, '_blank', 'location=yes,height=900,width=700,scrollbars=yes,status=yes'); // Open new window
  }

  save(): void
  {
    this.api.updateBulkMailTemplate_Command({
      eventId: this.eventService.selectedId,
      templateId: this.templateForm.value.id,
      senderName: this.templateForm.value.senderName,
      senderMail: this.templateForm.value.senderMail,
      subject: this.templateForm.value.subject,
      contentHtml: this.templateForm.value.contentHtml,
      audiences: this.selectedAudiences,
      registrableIds: this.registrableIds,
      addIcs: this.templateForm.value.addIcs
    })
      .subscribe();
  }

  generateMails(): void
  {
    this.service.generateMails(this.bulkMailKey);
  }

  releaseMails(): void
  {
    this.service.releaseMails(this.bulkMailKey);
  }
}
