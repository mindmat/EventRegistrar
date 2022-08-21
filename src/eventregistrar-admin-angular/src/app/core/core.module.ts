import { NgModule, Optional, SkipSelf } from '@angular/core';
import { IconsModule } from 'app/core/icons/icons.module';
import { AgoPipe } from './ago.pipe';
import { UserHasRightDirective } from './auth/user-has-right.directive';
// import { TranslocoCoreModule } from 'app/core/transloco/transloco.module';

@NgModule({
    imports: [
        IconsModule,
        // TranslocoCoreModule
    ],
    declarations: [
        AgoPipe,
        UserHasRightDirective
    ],
    exports: [
        AgoPipe,
        UserHasRightDirective
    ]
})
export class CoreModule
{
    /**
     * Constructor
     */
    constructor(
        @Optional() @SkipSelf() parentModule?: CoreModule
    )
    {
        // Do not allow multiple injections
        if (parentModule)
        {
            throw new Error('CoreModule has already been loaded. Import this module in the AppModule only.');
        }
    }
}
