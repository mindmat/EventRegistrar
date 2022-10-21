import { Overlay, OverlayRef } from '@angular/cdk/overlay';
import { TemplatePortal } from '@angular/cdk/portal';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, Input, OnChanges, OnInit, Renderer2, SimpleChanges, TemplateRef, ViewChild, ViewContainerRef } from '@angular/core';
import { getMatIconNameNotFoundError } from '@angular/material/icon';
import { BehaviorSubject, throwIfEmpty } from 'rxjs';

@Component({
  selector: 'app-tags-picker',
  templateUrl: './tags-picker.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TagsPickerComponent implements OnInit, OnChanges
{
  @ViewChild('tagsPanel') private _tagsPanel: TemplateRef<any>;
  @ViewChild('tagsPanelOrigin') private _tagsPanelOrigin: ElementRef;

  @Input() allTags: Tag[];
  //   (tags: any[])
  // {
  //   console.log(tags);
  //   console.log(this.allTags);
  //   this.changeDetectorRef.markForCheck();
  // }
  // public allTags: Tag[]
  @Input() selectedTagIds: string[] = [];
  @Input() textProperty: string = 'name';

  //   (tags: string[])
  // {
  //   this.changeDetectorRef.markForCheck();
  // }
  public filteredTags: any[];
  private tagsPanelOverlayRef: OverlayRef;
  private tagFilter$: BehaviorSubject<string> = new BehaviorSubject<string>('');

  constructor(private changeDetectorRef: ChangeDetectorRef,
    private overlay: Overlay,
    private renderer2: Renderer2,
    private viewContainerRef: ViewContainerRef
  ) { }

  ngOnChanges(changes: SimpleChanges): void
  {
    if (changes['allTags'])
    {
      this.applyTagFilter();
    }
  }

  ngOnInit(): void
  {
    this.tagFilter$.subscribe(_ => this.applyTagFilter());
  }

  applyTagFilter(): void
  {
    var value = this.tagFilter$.value;
    this.filteredTags = this.allTags.filter(tag => tag[this.textProperty].toLowerCase().includes(value));
  }

  filterTags(event): void
  {
    const value = event.target.value.toLowerCase();
    this.tagFilter$.next(value);
    this.applyTagFilter();
  }

  filterTagsInputKeyDown(event): void
  {
    if (event.key !== 'Enter'
      || this.filteredTags.length === 0)
    {
      return;
    }

    // If there is a tag...
    const tag = this.filteredTags[0];
    const isTagApplied = this.selectedTagIds.find(id => id === tag.id);

    // If the found tag is already applied to the contact...
    if (isTagApplied)
    {
      // Remove the tag from the contact
      this.removeTag(tag);
    }
    else
    {
      // Otherwise add the tag to the contact
      this.addTag(tag);
    }
  }

  addTag(tag: Tag): void
  {
    // Add the tag
    this.selectedTagIds.unshift(tag.id);

    // Update the contact form
    // this.contactForm.get('tags').patchValue(this.contact.tags);

    // Mark for check
    this.changeDetectorRef.markForCheck();
  }

  removeTag(tag: Tag): void
  {
    // Remove the tag
    this.selectedTagIds.splice(this.selectedTagIds.findIndex(id => id === tag.id), 1);

    // Update the contact form
    // this.contactForm.get('tags').patchValue(this.contact.tags);

    // Mark for check
    this.changeDetectorRef.markForCheck();
  }

  toggleTag(tag: Tag): void
  {
    if (this.selectedTagIds.includes(tag.id))
    {
      this.removeTag(tag);
    }
    else
    {
      this.addTag(tag);
    }
  }

  trackByFn(index: number, item: any): any
  {
    return item.id || index;
  }

  openTagsPanel(): void
  {
    // Create the overlay
    this.tagsPanelOverlayRef = this.overlay.create({
      backdropClass: '',
      hasBackdrop: true,
      scrollStrategy: this.overlay.scrollStrategies.block(),
      positionStrategy: this.overlay.position()
        .flexibleConnectedTo(this._tagsPanelOrigin.nativeElement)
        .withFlexibleDimensions(true)
        .withViewportMargin(64)
        .withLockedPosition(true)
        .withPositions([
          {
            originX: 'start',
            originY: 'bottom',
            overlayX: 'start',
            overlayY: 'top'
          }
        ])
    });

    // Subscribe to the attachments observable
    this.tagsPanelOverlayRef.attachments().subscribe(() =>
    {

      // Add a class to the origin
      this.renderer2.addClass(this._tagsPanelOrigin.nativeElement, 'panel-opened');

      // Focus to the search input once the overlay has been attached
      this.tagsPanelOverlayRef.overlayElement.querySelector('input').focus();
    });

    // Create a portal from the template
    const templatePortal = new TemplatePortal(this._tagsPanel, this.viewContainerRef);

    // Attach the portal to the overlay
    this.tagsPanelOverlayRef.attach(templatePortal);

    // Subscribe to the backdrop click
    this.tagsPanelOverlayRef.backdropClick().subscribe(() =>
    {

      // Remove the class from the origin
      this.renderer2.removeClass(this._tagsPanelOrigin.nativeElement, 'panel-opened');

      // If overlay exists and attached...
      if (this.tagsPanelOverlayRef && this.tagsPanelOverlayRef.hasAttached())
      {
        // Detach it
        this.tagsPanelOverlayRef.detach();

        // Reset the tag filter
        this.filteredTags = this.allTags;
      }

      // If template portal exists and attached...
      if (templatePortal && templatePortal.isAttached)
      {
        // Detach it
        templatePortal.detach();
      }
    });
  }
}

export interface Tag
{
  id?: string;
}
