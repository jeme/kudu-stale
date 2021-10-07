import { Component, OnInit } from '@angular/core';
import {IApplication} from "../../core/services/ApplicationsService";

@Component({
  selector: 'kudu-deployment-details',
  templateUrl: './deployment-details.component.html',
  styleUrls: ['./deployment-details.component.scss']
})
export class DeploymentDetailsComponent implements OnInit {
  application?: IApplication;

  constructor() { }

  ngOnInit(): void {
  }

}
