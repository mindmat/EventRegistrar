import { Pipe, PipeTransform } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

@Pipe({ name: 'translateEnum' })
export class TranslateEnumPipe implements PipeTransform
{
  constructor(private translateService: TranslateService) { }

  public transform(value: any, enumType: any, enumName: string): string
  {
    var enumValue = Object.values(enumType)[value];
    if (enumValue === '' || enumValue == undefined)
    {
      return null;
    }
    var translated = this.translateService.instant(`${enumName}_${enumValue}`);
    return translated;
  }
}