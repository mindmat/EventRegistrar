import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AutoMailTemplates, AutoMailTemplateMetadataLanguage, AutoMailTemplateMetadataType, MailSender } from 'app/api/api';
import { Subject, takeUntil } from 'rxjs';
import { AutoMailTemplatesService } from './auto-mail-templates.service';

@Component({
  selector: 'app-auto-mail-templates',
  templateUrl: './auto-mail-templates.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AutoMailTemplatesComponent implements OnInit
{
  templates: AutoMailTemplates;
  selectedTemplate: AutoMailTemplateMetadataLanguage;
  flagCodes: any;
  availableLangs = [
    { id: 'de', label: 'Deutsch' },
    { id: 'en', label: 'English' }
  ];

  configForm = this.fb.group({
    eventId: '',
    senderName: '',
    senderMail: '',
    singleRegistrationPossible: false,
    partnerRegistrationPossible: false,
    sendRegistrationReceivedMail: false,
    mailSender: MailSender.Smtp,
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
      this.configForm.value.smtpHost,
      this.configForm.value.smtpPort,
      this.configForm.value.smtpUsername,
      this.configForm.value.smtpPassword);
  }
}
