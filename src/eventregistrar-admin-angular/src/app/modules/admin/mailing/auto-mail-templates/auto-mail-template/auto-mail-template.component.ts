import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Api, AutoMailTemplateDisplayItem, MailType, PlaceholderDescription } from 'app/api/api';
import { EventService } from 'app/modules/admin/events/event.service';
import { Subject, takeUntil } from 'rxjs';
import { AutoMailTemplateService } from './auto-mail-template.service';
import { TributeItem } from 'tributejs';

import Tribute from "tributejs";
import FroalaEditor from "froala-editor";
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-auto-mail-template',
  templateUrl: './auto-mail-template.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AutoMailTemplateComponent implements OnInit
{
  editorRef: FroalaEditor;
  @ViewChild('editor', { static: false }) editor: ElementRef<HTMLElement>;

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

  public options = null;

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

    FroalaEditor.DefineIcon('insertTag', { NAME: 'mention', SVG_KEY: 'mention' });
    FroalaEditor.RegisterCommand('insertTag', {
      title: this.translateService.instant('InsertPlaceholder'),
      focus: true,
      undo: true,
      refreshAfterCallback: true,
      callback: function ()
      {
        const froalaEditor = this;
        froalaEditor.tribute.showMenuForCollection(froalaEditor.editor.nativeElement);
      }
    });

    this.api.froalaKey_Query({}).subscribe(key =>
    {
      this.options = {
        language: this.translateService.currentLang,
        htmlRemoveTags: [],
        key: key,
        toolbarButtons: {

          'moreText': {
            'buttons': ['bold', 'italic', 'underline', 'strikeThrough', 'subscript', 'superscript', 'fontFamily', 'fontSize', 'textColor', 'backgroundColor', 'inlineClass', 'inlineStyle', 'clearFormatting']
          },

          'moreParagraph': {
            'buttons': ['alignLeft', 'alignCenter', 'formatOLSimple', 'alignRight', 'alignJustify', 'formatOL', 'formatUL', 'paragraphFormat', 'paragraphStyle', 'lineHeight', 'outdent', 'indent', 'quote']
          },

          'moreRich': {
            'buttons': ['insertTag', 'insertLink', 'insertImage', 'insertTable', 'insertHR'],
            'buttonsVisible': 1
          },

          'moreMisc': {
            'buttons': ['undo', 'redo', 'fullscreen', 'selectAll', 'html', 'help'],
            'align': 'right',
            'buttonsVisible': 2
          }
        },
        events: {
          initialized: e =>
          {
            this.editorRef = e.getEditor();
            this.editorRef.tribute = this.tribute;
            this.editorRef.editor = this.editor;
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
    var url = `${this.eventService.selected.acronym}/mail-template-preview/${this.templateForm.value.id}`;
    window.open(url, '_blank', 'location=yes,height=900,width=700,scrollbars=yes,status=yes'); // Open new window
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
      .subscribe();
  }
}
