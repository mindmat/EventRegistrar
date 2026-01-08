import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { MailTypeItem, PendingMailListItem, MailType } from 'app/api/api';
import { BehaviorSubject, combineLatest, Subject, takeUntil } from 'rxjs';
import { ReleaseMailsService } from './release-mails.service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { AgoPipe } from 'app/modules/admin/infrastructure/ago.pipe';
import { MatSidenavModule } from "@angular/material/sidenav";

@Component({
  selector: 'app-release-mails',
  templateUrl: './release-mails.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [CommonModule, RouterModule, MatButtonModule, MatIconModule, MatMenuModule, MatFormFieldModule, MatInputModule, MatSelectModule, TranslateModule, AgoPipe, MatSidenavModule]
})
export class ReleaseMailsComponent implements OnInit
{
  pendingMails: PendingMailListItem[];
  filteredPendingMails: PendingMailListItem[];
  selectedMail: PendingMailListItem;
  query$: BehaviorSubject<string | null> = new BehaviorSubject(null);
  mailTypeFilter$: BehaviorSubject<MailType | null> = new BehaviorSubject(null);
  selectedMailType: MailType | null = null;
  availableMailTypes: MailTypeItem[] = [];
  private unsubscribeAll: Subject<any> = new Subject<any>();

  constructor(private service: ReleaseMailsService,
    private translateService: TranslateService,
    private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    combineLatest([this.query$, this.mailTypeFilter$, this.service.pendingMails$])
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe(([query, mailType, mails]) =>
      {
        this.pendingMails = mails;

        // Filter available mail types to only show those present in pending mails
        const presentMailTypes = new Set(mails.map(mail => mail.type).filter(type => type !== undefined && type !== null));
        this.availableMailTypes = Array.from(presentMailTypes).map(type => ({
          type,
          userText: `${this.translateService.instant('MailType_' + MailType[type])} (${mails.filter(mail => mail.type === type).length})`
        } as MailTypeItem));

        this.filteredPendingMails = mails;

        // Filter by search query
        if (!!query)
        {
          this.filteredPendingMails = this.filteredPendingMails.filter(
            mail => mail.recipientsEmails?.toLowerCase().includes(query.toLowerCase())
              || mail.recipientsNames?.toLowerCase().includes(query.toLowerCase())
              || mail.subject?.toLowerCase().includes(query.toLowerCase()));
        }

        // Filter by mail type
        if (mailType !== null)
        {
          this.filteredPendingMails = this.filteredPendingMails.filter(mail => mail.type === mailType);
        }

        // Mark for check
        this.changeDetectorRef.markForCheck();
      });
  }

  filterChats(query: string): void
  {
    this.query$.next(query);
  }

  onMailSelected(mail: PendingMailListItem): void
  {
    this.selectedMail = mail;
  }

  releaseAll(): void
  {
    this.service.releaseMails(this.filteredPendingMails.map(mail => mail.id));
  }

  deleteAll(): void
  {
    this.service.deleteMails(this.filteredPendingMails.map(mail => mail.id));
  }

  onMailTypeFilterChange(mailType: MailType | null): void
  {
    this.selectedMailType = mailType;
    this.mailTypeFilter$.next(mailType);
  }

  trackByFn(index: number, item: any): any
  {
    return item.id || index;
  }
}
