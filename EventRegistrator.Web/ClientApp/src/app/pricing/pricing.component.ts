import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { EventService } from "../events/eventService.service";
import { DoubleRegistrable, SingleRegistrable, Registrable } from '../registrables/registrables.component';
import { Guid } from '../infrastructure/guid';

@Component({
  selector: 'pricing',
  templateUrl: './pricing.component.html'
})
export class PricingComponent implements OnInit {
  doubleRegistrables: Registrable[];
  singleRegistrables: Registrable[];
  registrables: Registrable[];
  dropdownSettings = {};

  constructor(private http: HttpClient, private route: ActivatedRoute, private eventService: EventService) {
  }

  ngOnInit() {
    this.dropdownSettings = {
      placeholder: 'Reduktion',
      singleSelection: true,
      idField: 'id',
      textField: 'name',
      allowSearchFilter: true
    };

    this.http.get<DoubleRegistrable[]>(`api/events/${this.getEventAcronym()}/DoubleRegistrableOverview`).subscribe(result => {
      this.doubleRegistrables = result;
      this.fillRegistrables();
    }, error => console.error(error));
    this.http.get<SingleRegistrable[]>(`api/events/${this.getEventAcronym()}/SingleRegistrableOverview`).subscribe(result => {
      this.singleRegistrables = result;
      this.fillRegistrables();
    }, error => console.error(error));

    this.refresh();
  }
  private refresh() {
    this.http.get<RegistrablePricing[]>(`api/events/${this.getEventAcronym()}/registrables/pricing`).subscribe(result => {
      this.pricings = result;
    }, error => console.error(error));
  }

  private fillRegistrables() {
    if (this.singleRegistrables != null && this.doubleRegistrables != null) {
      this.registrables = this.singleRegistrables.concat(this.doubleRegistrables);
    }
  }

  private addReduction(pricing: RegistrablePricing) {
    var newReduction: PricingReduction = { id: Guid.newGuid(), amount: 0, registrableId1_ReductionActivatedIfCombinedWith: null, registrableId2_ReductionActivatedIfCombinedWith: null };
    pricing.reductions.push(newReduction);
  }

  private removeReduction(pricing: RegistrablePricing, reduction: PricingReduction) {
    var index = pricing.reductions.indexOf(reduction);
    pricing.reductions.splice(index,1);
  }

  pricings: RegistrablePricing[];

  getEventAcronym() {
    return this.route.snapshot.params['eventAcronym'];
  }
}

export class RegistrablePricing {
  registrableId: string;
  registrableName: string;
  price: number;
  reducedPrice: number;
  reductions: PricingReduction[];
}

export class PricingReduction {
  id: string;
  amount: number;
  registrableId1_ReductionActivatedIfCombinedWith: string;
  registrableId2_ReductionActivatedIfCombinedWith: string;
}
