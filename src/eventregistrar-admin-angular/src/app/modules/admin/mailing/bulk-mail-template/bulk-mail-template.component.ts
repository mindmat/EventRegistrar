import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { TranslateModule } from '@ngx-translate/core';
import { Api, BulkMailTemplateDisplayItem, GeneratedBulkMails, MailingAudience, PlaceholderDescription, PossibleAudience } from 'app/api/api';
import { FroalaEditorModule } from 'angular-froala-wysiwyg';
import FroalaEditor from 'froala-editor';
import { Subject, takeUntil } from 'rxjs';
import Tribute, { TributeItem } from 'tributejs';
import { EventService } from '../../events/event.service';
import { RegistrablesService } from '../../pricing/registrables.service';
import { BulkMailTemplateService } from './bulk-mail-template.service';
import { GeneratedBulkMailsService } from './generated-bulk-mails.service';
import { TagsPickerComponent } from 'app/shared/tags-picker/tags-picker.component';

@Component({
  selector: 'app-bulk-mail-template',
  templateUrl: './bulk-mail-template.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    MatFormFieldModule,
    MatInputModule,
    MatSlideToggleModule,
    MatButtonModule,
    TagsPickerComponent,
    FroalaEditorModule
  ]
})
export class BulkMailTemplateComponent implements OnInit
{
  @ViewChild('editor', { static: false }) editor: ElementRef<HTMLElement>;
  editorRef: FroalaEditor;

  possibleAudiences: PossibleAudience[];
  selectedAudiences: MailingAudience[] | null;
  registrableIds: string[] | null;
  mailsProgress: GeneratedBulkMails;

  templateForm = this.fb.group({
    id: '',
    senderName: '',
    senderMail: '',
    subject: '',
    contentHtml: '',
    addIcs: false,
  });
  public options = null;

  private unsubscribeAll: Subject<any> = new Subject<any>();
  private placeholders: PlaceholderDescription[];
  private initialHtml: string | null;
  private bulkMailKey: string;

  private tribute = new Tribute(
    {
      values: (text, cb): void => { cb(this.placeholders.filter(plh => plh.description.toLowerCase().includes(text.toLowerCase()))); },
      lookup: 'description',

      // function called on select that returns the content to insert
      selectTemplate: (item: TributeItem<PlaceholderDescription>): string => item.original.placeholder,

      // template for displaying item in menu
      menuItemTemplate: (item: TributeItem<PlaceholderDescription>): string => item.original.description,
    });

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

        if (this.editorRef)
        {
          this.editorRef.html.set(template?.contentHtml);
        }
        else
        {
          this.initialHtml = template?.contentHtml;
        }

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });

    this.service.getAvailablePlaceholders()
      .subscribe(placeholders => this.placeholders = placeholders);

    this.service.getAvailableAudiences()
      .subscribe(audiences => this.possibleAudiences = audiences);

    this.api.froalaKey_Query({}).subscribe((key) =>
    {
      this.options = {
        htmlRemoveTags: [],
        key: key,
        events: {
          initialized: (e): void =>
          {
            this.editorRef = e.getEditor();
            this.tribute.attach(this.editor.nativeElement);
            if (this.initialHtml)
            {
              this.editorRef.html.set(this.initialHtml);
              this.changeDetectorRef.markForCheck();
            }
            // pick mention with Enter, don't propagate to the html editor
            this.editor.nativeElement.addEventListener('keydown', (eventKeydown): boolean =>
            {
              if (eventKeydown.key === FroalaEditor.KEYCODE.ENTER && this.tribute.isActive)
              {
                return false;
              }
            }, true);
          }
        }
      };
      this.changeDetectorRef.markForCheck();
    });

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
    const html = this.editorRef.html.get(true);
    this.api.updateBulkMailTemplate_Command({
      eventId: this.eventService.selectedId,
      templateId: this.templateForm.value.id,
      senderName: this.templateForm.value.senderName,
      senderMail: this.templateForm.value.senderMail,
      subject: this.templateForm.value.subject,
      contentHtml: html,
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
