import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { IndividualReductionType, SpotDisplayItem, RegistrableDisplayItem } from 'app/api/api';
import { RegistrablesService } from '../../pricing/registrables.service';
import { SpotsService } from '../spots/spots.service';

@Component({
  selector: 'app-change-spots',
  templateUrl: './change-spots.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChangeSpotsComponent implements OnInit
{
  spotsOfRegistration: RegistrableSpot[];
  availableRegistrables: AvailableRegistrable[];
  IndividualReductionType = IndividualReductionType;

  private bookedSpotIds: string[];
  private registrables: RegistrableDisplayItem[];

  constructor(private changeDetectorRef: ChangeDetectorRef,
    @Inject(MAT_DIALOG_DATA) public data: { registrationId: string; spots?: SpotDisplayItem[]; },
    private registrablesService: RegistrablesService,
    private spotsService: SpotsService,
    public matDialogRef: MatDialogRef<ChangeSpotsComponent>) { }

  ngOnInit(): void
  {
    this.bookedSpotIds = this.data.spots?.map(spt => spt.registrableId);
    this.registrablesService.registrables$.subscribe((rbl) => { this.registrables = rbl; this.updateList(); });
    this.registrablesService.fetchRegistrables().subscribe();
    this.updateList();
  }

  public updateSpots(spots: SpotDisplayItem[] | null): void
  {
    this.bookedSpotIds = spots?.map(spt => spt.registrableId);
    this.updateList();
  }

  addSpot(registrableId: string, asFollower: boolean = false): void
  {
    this.spotsService.addSpot(this.data.registrationId, registrableId, asFollower);
  }

  removeSpot(registrableId: string): void
  {
    this.spotsService.removeSpot(this.data.registrationId, registrableId);
  }

  private updateList(): void
  {
    this.spotsOfRegistration = this.registrables?.filter(rbl => this.bookedSpotIds?.includes(rbl.id) === true)
      .map(rbl =>
        ({
          registrableId: rbl.id,
          name: rbl.name,
          nameSecondary: rbl.nameSecondary
        }) as RegistrableSpot);

    this.availableRegistrables = this.registrables?.filter(rbl => this.bookedSpotIds?.includes(rbl.id) !== true)
      .map(rbl =>
        ({
          registrableId: rbl.id,
          name: rbl.name,
          nameSecondary: rbl.nameSecondary,
          isPartnerRegistrable: rbl.isDoubleRegistrable
        }) as AvailableRegistrable);

    this.changeDetectorRef.markForCheck();
  }
}

class RegistrableSpot
{
  registrableId: string;
  name: string;
  nameSecondary: string;
}

class AvailableRegistrable
{
  registrableId: string;
  name: string;
  nameSecondary: string;
  isPartnerRegistrable: boolean;
}
