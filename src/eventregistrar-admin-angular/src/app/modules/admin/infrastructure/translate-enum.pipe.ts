import { Pipe, PipeTransform } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

@Pipe({ standalone: false, name: 'translateEnum' })
export class TranslateEnumPipe implements PipeTransform
{
  constructor(private translateService: TranslateService) { }

  public transform(value: any, enumType: any, enumName: string): string
  {
    const index = Object.keys(enumType).indexOf(`${value}`);
    const enumValue = Object.values(enumType)[index];
    if (enumValue === '' || enumValue === undefined)
    {
      return null;
    }
    const translated = this.translateService.instant(`${enumName}_${enumValue}`);
    return translated;
  }
}
