import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { MatSidenavModule } from '@angular/material/sidenav';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { BulkMailTemplateMetadataLanguage, BulkMailTemplates, BulkMailTemplateKey } from 'app/api/api';
import { Subject, takeUntil } from 'rxjs';
import { BulkMailTemplatesService } from './bulk-mail-templates.service';

@Component({
  selector: 'app-bulk-mail-templates',
  templateUrl: './bulk-mail-templates.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    TranslateModule,
    MatSidenavModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule
  ]
})
export class BulkMailTemplatesComponent implements OnInit
{
  templates: BulkMailTemplates;
  selectedTemplate: BulkMailTemplateMetadataLanguage;
  drawerOpened: boolean = false;

  flagCodes: any;
  availableLangs = [
    { id: 'de', label: 'Deutsch', flag: 'de' },
    { id: 'en', label: 'English', flag: 'us' }
  ];
  private unsubscribeAll: Subject<any> = new Subject<any>();

  constructor(
    private service: BulkMailTemplatesService,
    private route: ActivatedRoute,
    private router: Router,
    private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    this.flagCodes = {
      'de': 'de',
      'en': 'us'
    };

    this.service.bulkMailTemplates$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((templates: BulkMailTemplates) =>
      {
        this.templates = templates;

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });

  };

  createNewBulkMail(key: string): void
  {
    this.service.createTemplate(key);
    this.drawerOpened = false;
  }

  selectTemplate(template: BulkMailTemplateMetadataLanguage, type: BulkMailTemplateKey): void
  {
    this.selectedTemplate = template;
    this.router.navigate([`./${template.id}`], { relativeTo: this.route });

    // Mark for check
    this.changeDetectorRef.markForCheck();
  }

  deleteTemplate(key: string): void
  {
    this.service.deleteTemplate(key);
  }
}
