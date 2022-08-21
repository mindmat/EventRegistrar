import { Directive, Input, OnDestroy, TemplateRef, ViewContainerRef } from '@angular/core';
import { BehaviorSubject, Subscription, combineLatest, map, tap } from 'rxjs';
import { RightsService } from './rights.service';

@Directive({
  selector: '[appUserHasRight]',
  exportAs: 'authorization'
})
export class UserHasRightDirective implements OnDestroy
{
  private right$ = new BehaviorSubject<string>(null);
  private fallback$ = new BehaviorSubject<TemplateRef<any>>(null);
  private subscription: Subscription;

  constructor(
    private templateRef: TemplateRef<any>,
    private viewContainer: ViewContainerRef,
    private rightsService: RightsService,
  )
  {
    this.subscription = combineLatest([this.rightsService.rights$, this.right$, this.fallback$]).pipe(
      map(([rights, right, fallback]) => ({ authorized: rights?.has(right), fallback }))
    ).subscribe(({ authorized, fallback }) =>
    {
      this.viewContainer.clear();
      if (authorized)
      {
        this.viewContainer.createEmbeddedView(this.templateRef);
      } else if (fallback)
      {
        this.viewContainer.createEmbeddedView(fallback);
      }
    });
  }

  @Input('appUserHasRight') set right(right: string)
  {
    this.right$.next(right);
  }

  @Input() set appHasAuthorizationFallback(fallback: TemplateRef<any>)
  {
    this.fallback$.next(fallback);
  }

  get right()
  {
    return this.right$.value;
  }

  ngOnDestroy(): void
  {
    this.subscription.unsubscribe();
  }
}
