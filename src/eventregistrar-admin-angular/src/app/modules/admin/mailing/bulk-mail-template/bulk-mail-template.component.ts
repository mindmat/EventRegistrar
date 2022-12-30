import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Api, BulkMailTemplateDisplayItem, MailingAudience, PlaceholderDescription, PossibleAudience } from 'app/api/api';
import { Subject, takeUntil } from 'rxjs';
import { EventService } from '../../events/event.service';
import { BulkMailTemplateService } from './bulk-mail-template.service';

import Tribute, { TributeItem } from "tributejs";
import FroalaEditor from "froala-editor";
import { RegistrablesService } from '../../pricing/registrables.service';

@Component({
  selector: 'app-bulk-mail-template',
  templateUrl: './bulk-mail-template.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BulkMailTemplateComponent implements OnInit
{
  editorRef: FroalaEditor;
  @ViewChild('editor', { static: false }) editor: ElementRef<HTMLElement>;

  private unsubscribeAll: Subject<any> = new Subject<any>();
  private placeholders: PlaceholderDescription[];
  possibleAudiences: PossibleAudience[];
  private initialHtml: string | null;
  selectedAudiences: MailingAudience[];
  registrableId: string | null;

  templateForm = this.fb.group({
    id: '',
    subject: '',
    contentHtml: ''
  });

  private tribute = new Tribute(
    {
      values: (text, cb) => { cb(this.placeholders.filter(plh => plh.description.toLowerCase().includes(text.toLowerCase()))); },
      lookup: 'description',

      // function called on select that returns the content to insert
      selectTemplate: (item: TributeItem<PlaceholderDescription>) =>
      {
        return item.original.placeholder;
      },

      // template for displaying item in menu
      menuItemTemplate: (item: TributeItem<PlaceholderDescription>) =>
      {
        return item.original.description;
      },
    });

  public options = null;

  constructor(private service: BulkMailTemplateService,
    private fb: FormBuilder,
    private api: Api,
    private eventService: EventService,
    public registrablesService: RegistrablesService,
    private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    this.service.template$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((template: BulkMailTemplateDisplayItem) =>
      {
        this.templateForm.patchValue(template);
        this.selectedAudiences = template.audiences;
        this.registrableId = template.registrableId;

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

    this.api.froalaKey_Query({}).subscribe(key =>
    {
      this.options = {
        htmlRemoveTags: [],
        key: key,
        events: {
          initialized: e =>
          {
            this.editorRef = e.getEditor();
            this.tribute.attach(this.editor.nativeElement);
            if (this.initialHtml)
            {
              this.editorRef.html.set(this.initialHtml);
              this.changeDetectorRef.markForCheck();
            }
            // pick mention with Enter, don't propagate to the html editor 
            this.editor.nativeElement.addEventListener('keydown', e =>
            {
              if (e.key == FroalaEditor.KEYCODE.ENTER && this.tribute.isActive)
              {
                return false;
              }
            }, true);
          }
        }
      };
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
    let html = this.editorRef.html.get(true);
    this.api.updateBulkMailTemplate_Command({
      eventId: this.eventService.selectedId,
      templateId: this.templateForm.value.id,
      subject: this.templateForm.value.subject,
      contentHtml: html,
      audiences: this.selectedAudiences,
      registrableId: this.registrableId
    })
      .subscribe();
  }
}
