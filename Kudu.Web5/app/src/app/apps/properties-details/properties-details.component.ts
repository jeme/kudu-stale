import { Component, OnInit } from '@angular/core';
import {IApplication} from "../../core/services/ApplicationsService";

@Component({
  selector: 'kudu-properties-details',
  templateUrl: './properties-details.component.html',
  styleUrls: ['./properties-details.component.scss']
})
export class PropertiesDetailsComponent implements OnInit {
  application?: IApplication;

  constructor() { }

  ngOnInit(): void {
  }

}
