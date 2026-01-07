import { Injectable } from '@angular/core';
import { RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { FixRawProcessingService } from './fix-raw-processing.service';

@Injectable({
  providedIn: 'root'
})
export class FixRawProcessingResolver  
{
  constructor(private fixService: FixRawProcessingService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean>
  {
    return this.fixService.fetchErrors();
  }
}
