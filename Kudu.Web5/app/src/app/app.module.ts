import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import {MatTabsModule} from '@angular/material/tabs';
import {MatInputModule} from '@angular/material/input';
import {MatSnackBarModule} from '@angular/material/snack-bar';

import { CoreModule } from "./core/core.module";

import { ApplicationsComponent } from './applications/applications.component';

import { ApplicationDetailsComponent } from './apps/application-details/application-details.component';
import { CreateApplicationComponent } from './create-application/create-application.component';
import { BindingDetailsComponent } from './apps/binding-details/binding-details.component';
import { ConfigurationDetailsComponent } from './apps/configuration-details/configuration-details.component';
import { DeploymentDetailsComponent } from './apps/deployment-details/deployment-details.component';
import {FormsModule} from "@angular/forms";
import {PropertiesDetailsComponent} from "./apps/properties-details/properties-details.component";
import {MatCardModule} from "@angular/material/card";

@NgModule({
  declarations: [
    AppComponent,
    ApplicationsComponent,
    ApplicationDetailsComponent,
    CreateApplicationComponent,
    BindingDetailsComponent,
    ConfigurationDetailsComponent,
    DeploymentDetailsComponent,
    PropertiesDetailsComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,

    MatIconModule,
    MatToolbarModule,
    MatButtonModule,
    MatListModule,
    MatTabsModule,
    MatInputModule,
    MatSnackBarModule,
    MatCardModule,

    CoreModule,
    FormsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
