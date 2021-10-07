import { Component, OnInit } from '@angular/core';
import {IApplication} from "../../core/services/ApplicationsService";

@Component({
  selector: 'kudu-binding-details',
  templateUrl: './binding-details.component.html',
  styleUrls: ['./binding-details.component.scss']
})
export class BindingDetailsComponent implements OnInit {
  application?: IApplication;

  constructor() { }

  ngOnInit(): void {
  }

}
