import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, Inject, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Api, RegistrableIcsItem } from 'app/api/api';
import { RegistrableDetailComponent } from '../../overview/registrable-detail/registrable-detail.component';
import { RegistrableIcsService } from './registrable-ics.service';

import FroalaEditor from "froala-editor";
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-registrable-ics',
  templateUrl: './registrable-ics.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RegistrableIcsComponent implements OnInit
{
  editorRef: FroalaEditor;
  @ViewChild('editor', { static: false }) editor: ElementRef<HTMLElement>;
  public options = null;

  registrableForm: FormGroup;

  constructor(private changeDetectorRef: ChangeDetectorRef,
    @Inject(MAT_DIALOG_DATA) private data: { registrableId: string, name: string; },
    private registrablesService: RegistrableIcsService,
    public matDialogRef: MatDialogRef<RegistrableDetailComponent>,
    private fb: FormBuilder,
    private api: Api,
    private translateService: TranslateService) { }

  ngOnInit(): void
  {
    this.registrablesService.getRegistrableIcs(this.data.registrableId)
      .subscribe(ics =>
      {
        ics ??= {
          registrableId: this.data.registrableId,
          addToCalendar: false,
          location: '',
          start: new Date(),
          end: new Date(),
          contentHtml: ''
        };
        this.registrableForm = this.fb.group<RegistrableIcsItem>(ics);

        this.changeDetectorRef.markForCheck();
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
            this.editorRef.editor = this.editor;
            // if (this.initialHtml)
            // {
            //   this.editorRef.html.set(this.initialHtml);
            //   this.changeDetectorRef.markForCheck();
            // }
          }
        }
      };
      this.changeDetectorRef.markForCheck();
    });
  }

  onSubmit(): void
  {
    this.registrablesService.saveRegistrableIcs(this.registrableForm.value);
  }
}
