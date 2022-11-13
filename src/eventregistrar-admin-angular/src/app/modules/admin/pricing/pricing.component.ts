import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { PricePackageDto, PricePackagePartDto, PricePackagePartSelectionTypeOption, RegistrableDisplayItem } from 'app/api/api';
import { Subject, takeUntil } from 'rxjs';
import { PricingService } from './pricing.service';
import { RegistrablesService } from './registrables.service';
import { v4 as createUuid } from 'uuid';
import { FormArray, FormBuilder, FormGroup } from '@angular/forms';
import { PricePackagePartSelectionTypeService } from './pricing-selection-type.service';

@Component({
  selector: 'app-pricing',
  templateUrl: './pricing.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PricingComponent implements OnInit
{
  public packagesForms: FormGroup[] = [];
  private unsubscribeAll: Subject<any> = new Subject<any>();
  packages: PricePackageDto[];
  registrables: RegistrableDisplayItem[];
  selectionTypes: PricePackagePartSelectionTypeOption[];

  constructor(private pricingService: PricingService,
    private registrablesService: RegistrablesService,
    private pricePackagePartSelectionTypeService: PricePackagePartSelectionTypeService,
    private fb: FormBuilder,
    private changeDetectorRef: ChangeDetectorRef) { }

  ngOnInit(): void
  {
    this.pricingService.pricePackages$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((packages: PricePackageDto[]) =>
      {
        this.packages = packages;
        this.packagesForms = packages.map(ppg => this.fb.group(
          {
            ...ppg,
            parts: this.fb.array(ppg.parts?.map(ppp => this.fb.group(
              {
                ...ppp,
                registrableIds: this.fb.array(ppp.registrableIds)
              })))
          }));

        this.changeDetectorRef.markForCheck();
      });

    this.registrablesService.registrables$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((registrables: RegistrableDisplayItem[]) =>
      {
        this.registrables = registrables;

        this.changeDetectorRef.markForCheck();
      });

    this.pricePackagePartSelectionTypeService.selectionTypes$
      .pipe(takeUntil(this.unsubscribeAll))
      .subscribe((selectionTypes: PricePackagePartSelectionTypeOption[]) =>
      {
        this.selectionTypes = selectionTypes;

        this.changeDetectorRef.markForCheck();
      });
  }

  getParts(packageForm: FormGroup): FormArray<FormGroup>
  {
    return packageForm.controls.parts as FormArray;
  }

  addPackage()
  {
    this.packagesForms.push(this.fb.group({
      id: createUuid(),
      name: '',
      price: 0,
      parts: this.fb.array([])
    }));

    // this.changeDetectorRef.markForCheck();
  }

  addPart(packageForm: FormGroup)
  {
    this.getParts(packageForm).push(this.fb.group({
      id: createUuid(),
      isOptional: false,
      reduction: 0,
      registrableIds: this.fb.array([] as string[])
    }));

    // this.changeDetectorRef.markForCheck();
  }

  removePackage(index: number)
  {
    this.packagesForms.splice(index, 1);
  }

  removePackagePart(packageForm: FormGroup, index: number)
  {
    this.getParts(packageForm).removeAt(index);
  }

  save()
  {
    const packages = this.packagesForms.map(pkf => ({ ...pkf.value } as PricePackageDto));
    this.pricingService.save(packages);
  }
}