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
      singleSelection: false,
      idField: 'id',
      textField: 'name',
      itemsShowLimit: 2,
      limitSelection: 2,
      enableCheckAll: false
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
      this.pricings.forEach(prc => prc.reductions.forEach(red => red.registrables_Combined = this.registrables.filter(rbl => rbl.id == red.registrableId1_ReductionActivatedIfCombinedWith || rbl.id == red.registrableId2_ReductionActivatedIfCombinedWith)));
    }, error => console.error(error));
  }

  private fillRegistrables() {
    if (this.singleRegistrables != null && this.doubleRegistrables != null) {
      this.registrables = this.singleRegistrables.concat(this.doubleRegistrables);
    }
  }

  private addReduction(pricing: RegistrablePricing) {
    let newReduction: PricingReduction =
    {
      id: Guid.newGuid(),
      amount: 0,
      registrableId1_ReductionActivatedIfCombinedWith: null,
      registrableId2_ReductionActivatedIfCombinedWith: null,
      registrables_Combined: []
    };
    this.http.put(`api/events/${this.getEventAcronym()}/registrables/${pricing.registrableId}/reductions/${newReduction.id}`, newReduction).subscribe(result => {
      pricing.reductions.push(newReduction);
    }, error => console.error(error));
  }

  private removeReduction(pricing: RegistrablePricing, reduction: PricingReduction) {
    this.http.delete(`api/events/${this.getEventAcronym()}/registrables/${pricing.registrableId}/reductions/${reduction.id}`).subscribe(result => {
      var index = pricing.reductions.indexOf(reduction);
      pricing.reductions.splice(index, 1);
    }, error => console.error(error));
  }

  private saveReduction(registrableId: string, reduction: PricingReduction) {
    reduction.registrableId1_ReductionActivatedIfCombinedWith = reduction.registrables_Combined.length > 0 ? reduction.registrables_Combined[0].id : null;
    reduction.registrableId2_ReductionActivatedIfCombinedWith = reduction.registrables_Combined.length > 1 ? reduction.registrables_Combined[1].id : null;
    console.log(reduction);
    this.http.put(`api/events/${this.getEventAcronym()}/registrables/${registrableId}/reductions/${reduction.id}`, reduction).subscribe(result => {
    }, error => console.error(error));
  }

  private savePrices(pricing: RegistrablePricing) {
    let url = `api/events/${this.getEventAcronym()}/registrables/${pricing.registrableId}/prices`;
    if (pricing.price != null) {
      url += `?price=${pricing.price}`;
      if (pricing.reducedPrice != null) {
        url += `&reducedPrice=${pricing.reducedPrice}`;
      }
    }
    this.http.put(url, null).subscribe(result => {
    }, error => console.error(error));
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
  registrables_Combined: Registrable[];
  registrableId1_ReductionActivatedIfCombinedWith: string;
  registrableId2_ReductionActivatedIfCombinedWith: string;
}
