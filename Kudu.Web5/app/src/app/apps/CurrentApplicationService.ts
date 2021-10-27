import { Injectable } from '@angular/core';
import {IApplication} from "../core/services/ApplicationsService";
import {Subject} from "rxjs";

@Injectable()
export class CurrentApplicationService {
  public application?: IApplication;

  private applicationChanged = new Subject();
  public onApplicationChanged = this.applicationChanged.asObservable();

  update(application: IApplication) {
    this.application = application;
    this.applicationChanged.next();
  }
}
