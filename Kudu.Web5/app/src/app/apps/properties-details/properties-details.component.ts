import {Component, OnDestroy, OnInit} from '@angular/core';
import {IApplication} from "../../core/services/ApplicationsService";
import {Subscription} from "rxjs";
import {CurrentApplicationService} from "../CurrentApplicationService";

@Component({
  selector: 'kudu-properties-details',
  templateUrl: './properties-details.component.html',
  styleUrls: ['./properties-details.component.scss']
})
export class PropertiesDetailsComponent implements OnInit, OnDestroy {
  application?: IApplication;
  subscription: Subscription;

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
  }

}
