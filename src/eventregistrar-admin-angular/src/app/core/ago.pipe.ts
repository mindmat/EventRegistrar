import { AsyncPipe } from '@angular/common';
import { ChangeDetectorRef, Pipe } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { formatDistanceToNow, parseISO } from 'date-fns';
import { filter, combineLatest, distinctUntilChanged, map, Observable, ReplaySubject, tap, timer } from 'rxjs';
import { de, enUS } from 'date-fns/locale';

@Pipe({
  name: 'ago',
  pure: false
})
export class AgoPipe extends AsyncPipe
{
  private time: Date;
  private formatted$: Observable<string>;
  private input$: ReplaySubject<Date | number> = new ReplaySubject<Date | number>();

  constructor(cd: ChangeDetectorRef,
    private translateService: TranslateService)
  {
    super(cd);

    this.formatted$ = combineLatest([timer(0, 10000), this.input$]).pipe(
      filter(([_, input]) => !!input),
      map(([_, input]) => formatDistanceToNow(input, { addSuffix: true, includeSeconds: true, locale: this.mapLang(this.translateService.currentLang) })),
      distinctUntilChanged(),
      tap(time => console.log('new time:', time)),
    );
  }

  public transform(value: any): any
  {
    if (value as string)
    {
      value = parseISO(value);
    }
    this.input$.next(value);
    return super.transform(this.formatted$);
  }

  private mapLang(currentLang: string): Locale
  {
    if (currentLang == 'de')
    {
      return de;
    }
    return enUS;
  }
}


