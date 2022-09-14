import { ChangeDetectorRef, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormControl, FormGroup } from '@angular/forms';
import { Api, AutoMailTemplateDisplayItem, MailType, PlaceholderDescription } from 'app/api/api';
import { EventService } from 'app/modules/admin/events/event.service';
import { Observable, Subject, takeUntil } from 'rxjs';
import { AutoMailTemplateService } from './auto-mail-template.service';
import { TributeItem } from 'tributejs';

import Tribute from "tributejs";
import FroalaEditor from "froala-editor";

@Component({
  selector: 'app-auto-mail-template',
  templateUrl: './auto-mail-template.component.html'
})
export class AutoMailTemplateComponent implements OnInit
{
  editorRef: FroalaEditor;
  @ViewChild('editor', { static: true }) editor: ElementRef<HTMLElement>;

  private unsubscribeAll: Subject<any> = new Subject<any>();
  private placeholders: PlaceholderDescription[];
  private initialHtml: string | null;

  templateForm = this.fb.group<AutoMailTemplateDisplayItem>({
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

  public options = {
    htmlRemoveTags: [],
    events: {
      initialized: e =>
      {
        this.editorRef = e.getEditor();
        this.tribute.attach(this.editor.nativeElement);
        if (this.initialHtml)
        {
          this.editorRef.html.set(this.initialHtml);
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
        this.templateForm.patchValue(template);
        if (this.editorRef)
        {
          this.editorRef.html.set(template.contentHtml);
        }
        else
        {
          this.initialHtml = template.contentHtml;
        }

        this.updatePlaceholders(template.type);

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });
  }

  updatePlaceholders(type: MailType)
  {
    this.api.autoMailPlaceholder_Query({ mailType: type })
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((placeholders: PlaceholderDescription[]) =>
      {
        this.placeholders = placeholders;
      });
  }
  openPreview()
  {
    var registrationId = 'b450dc24-7591-4e45-8d64-36e62e375a78'; // ToDo
    var url = `auto-mail-preview/${this.templateForm.value.id}/registration/${registrationId}`;
    window.open(url, "_blank");
  }

  save()
  {
    let html = this.editorRef.html.get(true);
    this.api.updateAutoMailTemplate_Command({
      eventId: this.eventService.selectedId,
      templateId: this.templateForm.value.id,
      subject: this.templateForm.value.subject,
      contentHtml: html
    })
      .subscribe(x => console.log(x));
  }
}
