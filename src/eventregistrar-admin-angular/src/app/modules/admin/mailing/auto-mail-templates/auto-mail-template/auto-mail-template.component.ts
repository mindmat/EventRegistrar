import { ChangeDetectorRef, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { AutoMailTemplate } from 'app/api/api';
import { Editor, IEditor } from 'roosterjs';
import { Subject, takeUntil } from 'rxjs';
import { AutoMailTemplateService } from './auto-mail-template.service';

@Component({
  selector: 'app-auto-mail-template',
  templateUrl: './auto-mail-template.component.html'
})
export class AutoMailTemplateComponent implements OnInit
{
  private unsubscribeAll: Subject<any> = new Subject<any>();
  template: AutoMailTemplate;
  editor: IEditor;

  @ViewChild('editorTarget', { static: true }) editorTarget: ElementRef<HTMLDivElement>;

  constructor(private service: AutoMailTemplateService, private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    this.editor = new Editor(this.editorTarget.nativeElement, {
      // plugins: this.plugins,
      // initialContent: this._content,
      defaultFormat: {
        fontFamily: '"Inter var", sans-serif',
        fontSize: '1.4rem',        
      }
    });

    this.service.template$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((template: AutoMailTemplate) =>
      {
        this.template = template;
        this.editor.setContent(template.contentHtml, false);

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });
  }
}
