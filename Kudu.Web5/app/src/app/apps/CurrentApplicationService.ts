import { Injectable } from '@angular/core';
import {IApplication} from "../core/services/ApplicationsService";
import {Subject} from "rxjs";

@Injectable()
export class CurrentApplicationService {
  private applicationChanged = new Subject<IApplication>();
  public onApplicationChanged = this.applicationChanged.asObservable();

  update(application: IApplication) {
    this.applicationChanged.next(application);
  }
}
