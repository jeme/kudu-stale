import { Component, OnInit } from '@angular/core';
import {ApplicationsService, IApplication} from "../../core/services/ApplicationsService";
import { Router, ActivatedRoute, ParamMap } from '@angular/router';
import {CurrentApplicationService} from "../CurrentApplicationService";

@Component({
  selector: 'kudu-application-details',
  templateUrl: './application-details.component.html',
  styleUrls: ['./application-details.component.scss'],
  providers: [CurrentApplicationService]
})
export class ApplicationDetailsComponent implements OnInit {
  application?: IApplication;

  constructor(
    private applicationsService: ApplicationsService,
    private route: ActivatedRoute,
    private currentApplication: CurrentApplicationService) {}

  async ngOnInit() {
    this.route.paramMap.subscribe(async params => {
      if(params.has('name')){
        this.application = await this.applicationsService.get(<string>params.get('name'));
        this.currentApplication.update(this.application);
     }
    });
  }
}
