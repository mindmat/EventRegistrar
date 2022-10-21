import { Injectable } from '@angular/core';
import { Resolve, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable, of, zip } from 'rxjs';
import { FormsService } from './forms.service';
import { QuestionMappingService } from './question-mapping.service';
import { QuestionOptionMappingService } from './question-option-mapping.service';

@Injectable({
  providedIn: 'root'
})
export class FormMappingResolver implements Resolve<boolean>
{
  constructor(private formsService: FormsService,
    private questionOptionMappingService: QuestionOptionMappingService,
    private questionMappingService: QuestionMappingService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
  {
    return zip(
      this.formsService.fetchForms(),
      this.questionMappingService.fetchMappings(),
      this.questionOptionMappingService.fetchMappings()
    );
  }
}
