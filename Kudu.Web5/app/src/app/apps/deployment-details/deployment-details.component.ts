import {Component, OnDestroy, OnInit} from '@angular/core';
import {IApplication} from "../../core/services/ApplicationsService";
import {CurrentApplicationService} from "../CurrentApplicationService";
import {Subscription} from "rxjs";

@Component({
  selector: 'kudu-deployment-details',
  templateUrl: './deployment-details.component.html',
  styleUrls: ['./deployment-details.component.scss']
})
export class DeploymentDetailsComponent implements OnInit, OnDestroy {
  application?: IApplication;

  subscription: Subscription;

  msBuildArgs: string = "";

  constructor(private currentApplication: CurrentApplicationService) {
    this.application = currentApplication.application;
    this.onApplicationChanged();

    this.subscription = currentApplication.onApplicationChanged.subscribe(_ => {
      this.application = currentApplication.application;
      this.onApplicationChanged();
    });
  }


  ngOnInit(): void {  }

  ngOnDestroy(): void{
    this.subscription.unsubscribe();
  }

  private onApplicationChanged() {
    this.msBuildArgs = this.application?.settings.SCM_BUILD_ARGS || "";
  }
}
