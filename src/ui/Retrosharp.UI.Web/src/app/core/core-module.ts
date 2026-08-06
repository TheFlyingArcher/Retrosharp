import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavMenu } from './nav-menu/nav-menu';

import { MatMenuModule } from '@angular/material/menu'
import { MatToolbarModule } from '@angular/material/toolbar';

@NgModule({
  declarations: [NavMenu],
  imports: [CommonModule, MatMenuModule, MatToolbarModule],
})
export class CoreModule {}
