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

  constructor(private currentApplication: CurrentApplicationService) {
    this.subscription = currentApplication.onApplicationChanged.subscribe(value => {
      this.application = value;
    });
  }

  ngOnInit(): void {

  }

  ngOnDestroy(): void{
    this.subscription.unsubscribe();
  }
}
