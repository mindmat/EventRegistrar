import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatAccordion, MatExpansionModule } from '@angular/material/expansion';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDrawer, MatDrawerContainer, MatSidenavModule } from '@angular/material/sidenav';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { AutoMailTemplates, AutoMailTemplateMetadataLanguage, MailSender, MailSenderTokenKey, AutoMailTemplateMetadataType } from 'app/api/api';
import { Subject, takeUntil } from 'rxjs';
import { AutoMailTemplateComponent } from './auto-mail-template/auto-mail-template.component';
import { AutoMailTemplatesService } from './auto-mail-templates.service';
import { TranslateEnumPipe } from '../../infrastructure/translate-enum.pipe';

@Component({
  selector: 'app-auto-mail-templates',
  templateUrl: './auto-mail-templates.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    TranslateModule,
    MatSidenavModule,
    MatExpansionModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatSelectModule,
    MatChipsModule,
    MatCheckboxModule,
    TranslateEnumPipe,
    AutoMailTemplateComponent
  ]
})
export class AutoMailTemplatesComponent implements OnInit
{
  MailSenderTokenKey = MailSenderTokenKey;
  templates: AutoMailTemplates;
  selectedTemplate: AutoMailTemplateMetadataLanguage;
  flagCodes: any;
  availableLangs = [
    { id: 'de', label: 'Deutsch' },
    { id: 'en', label: 'English' }
  ];
  mailTokenKeys: MailSenderTokenKey[] = [MailSenderTokenKey.PostmarkToken, MailSenderTokenKey.PostmarkTokenSwima, MailSenderTokenKey.SendGridApiKey];

  configForm = this.fb.group({
    eventId: '',
    senderName: '',
    senderMail: '',
    singleRegistrationPossible: false,
    partnerRegistrationPossible: false,
    sendRegistrationReceivedMail: false,
    mailSender: MailSender.Smtp,
    mailSenderTokenKey: null as MailSenderTokenKey,
    smtpHost: null as string,
    smtpPort: null as number,
    smtpUsername: null as string,
    smtpPassword: null as string,
    availableLanguages: this.fb.array([] as string[])
  });
  MailSender = MailSender;
  public mailers: MailSender[];
  private unsubscribeAll: Subject<any> = new Subject<any>();

  constructor(
    private service: AutoMailTemplatesService,
    private route: ActivatedRoute,
    private router: Router,
    private changeDetectorRef: ChangeDetectorRef,
    private fb: FormBuilder) { }

  ngOnInit(): void
  {
    this.flagCodes = {
      'de': 'de',
      'en': 'us'
    };

    this.service.autoMailTemplates$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((templates: AutoMailTemplates) =>
      {
        this.templates = templates;
        this.configForm.patchValue({
          eventId: templates.eventId,
          senderName: templates.senderAlias,
          senderMail: templates.senderMail,
          singleRegistrationPossible: templates.singleRegistrationPossible,
          partnerRegistrationPossible: templates.partnerRegistrationPossible,
          sendRegistrationReceivedMail: templates.sendRegistrationReceivedMail,
          mailSender: templates.mailSender,
          mailSenderTokenKey: templates.mailSenderTokenKey,
          smtpHost: templates.smtpHost,
          smtpPort: templates.smtpPort,
          smtpUsername: templates.smtpUsername,
          availableLanguages: []
        });
        this.configForm.setControl('availableLanguages', this.fb.array(templates.availableLanguages));

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });

    this.service.getAvailableMailers().subscribe(
      (mailers: MailSender[]) => { this.mailers = mailers; }
    );
  }

  toggleLang(langId: string): void
  {
    const langs = this.configForm.value.availableLanguages;
    const index = langs.indexOf(langId);
    if (index < 0)
    {
      langs.push(langId);
    }
    else
    {
      langs.splice(index, 1);
    }
  }

  toggleSingle(): void
  {
    this.configForm.patchValue({
      singleRegistrationPossible: !this.configForm.value.singleRegistrationPossible
    });
  }

  togglePartner(): void
  {
    this.configForm.patchValue({
      partnerRegistrationPossible: !this.configForm.value.partnerRegistrationPossible
    });
  }

  toggleRelease(type: AutoMailTemplateMetadataType): void
  {
    type.releaseImmediately = !type.releaseImmediately;
    this.service.setReleaseMail(type.type, type.releaseImmediately);
  }

  toggleReceivedMail(): void
  {
    this.configForm.patchValue({
      sendRegistrationReceivedMail: !this.configForm.value.sendRegistrationReceivedMail
    });
  }

  selectTemplate(template: AutoMailTemplateMetadataLanguage, type: AutoMailTemplateMetadataType): void
  {
    this.selectedTemplate = template;

    if (template.id == null)
    {
      this.service.createTemplate(type.type, template.language)
        .subscribe(id => this.router.navigate([`./${id}`], { relativeTo: this.route }));
    }
    else
    {
      this.router.navigate([`./${template.id}`], { relativeTo: this.route });
    }

    // Mark for check
    this.changeDetectorRef.markForCheck();
  }

  saveSettings(): void
  {
    this.service.updateSettings(
      this.configForm.value.senderMail,
      this.configForm.value.senderName,
      this.configForm.value.availableLanguages,
      this.configForm.value.singleRegistrationPossible,
      this.configForm.value.partnerRegistrationPossible,
      this.configForm.value.sendRegistrationReceivedMail,
      this.configForm.value.mailSender,
      this.configForm.value.mailSenderTokenKey,
      this.configForm.value.smtpHost,
      this.configForm.value.smtpPort,
      this.configForm.value.smtpUsername,
      this.configForm.value.smtpPassword);
  }
}
