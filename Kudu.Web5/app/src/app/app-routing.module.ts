import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {ApplicationsComponent} from "./applications/applications.component";
import {ApplicationDetailsComponent} from "./application-details/application-details.component";
import {CreateApplicationComponent} from "./create-application/create-application.component";
import {BindingDetailsComponent} from "./apps/binding-details/binding-details.component";
import {DeploymentDetailsComponent} from "./apps/deployment-details/deployment-details.component";
import {ConfigurationDetailsComponent} from "./apps/configuration-details/configuration-details.component";

const routes: Routes = [
  { path: '', redirectTo: 'apps', pathMatch: 'full'},
  {
    path: 'apps', component: ApplicationsComponent,
    children: [
      {path: 'create', component: CreateApplicationComponent},
      //note: view/name is so that the name "Create" would not cause issues.
      {
        path: 'view/:name', component: ApplicationDetailsComponent,
        children: [
          { path: '', redirectTo: 'bindings', pathMatch: 'full'},
          { path: 'bindings', component: BindingDetailsComponent},
          { path: 'deployments', component: DeploymentDetailsComponent},
          { path: 'configuration', component: ConfigurationDetailsComponent},
        ]
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {
}
