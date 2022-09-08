import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { AutoMailTemplate } from 'app/api/api';
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

  constructor(private service: AutoMailTemplateService, private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    this.service.template$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((template: AutoMailTemplate) =>
      {
        this.template = template;

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });
  }
}
