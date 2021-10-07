import { Component, OnInit } from '@angular/core';
import {IApplication} from "../../core/services/ApplicationsService";

@Component({
  selector: 'kudu-configuration-details',
  templateUrl: './configuration-details.component.html',
  styleUrls: ['./configuration-details.component.scss']
})
export class ConfigurationDetailsComponent implements OnInit {
  application?: IApplication;

  constructor() { }

  ngOnInit(): void {
  }

}
