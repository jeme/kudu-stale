import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClientModule} from "@angular/common/http";
import {ApplicationsService} from "./services/ApplicationsService";



@NgModule({
  declarations: [],
  providers: [
    ApplicationsService
  ],
  imports: [
    CommonModule,
    HttpClientModule
  ]
})
export class CoreModule { }
