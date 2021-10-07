import { Component, OnInit } from '@angular/core';
import {ApplicationsService} from "../core/services/ApplicationsService";
import {
  MatSnackBar,
  MatSnackBarHorizontalPosition,
  MatSnackBarVerticalPosition,
} from '@angular/material/snack-bar';

@Component({
  selector: 'kudu-create-application',
  templateUrl: './create-application.component.html',
  styleUrls: ['./create-application.component.scss']
})
export class CreateApplicationComponent implements OnInit {
  applicationName: string | undefined;
  error: any;

  constructor(private applicationsService: ApplicationsService, private snackBar: MatSnackBar) { }

  ngOnInit(): void {
  }

  async onCreateClick() {
    if(!this.applicationName)
      return;

    const app = { name: <string>this.applicationName };
    try {
      await this.applicationsService.post(app);
    } catch (error){
      this.snackBar.open(error, 'OK', {
        horizontalPosition: "center",
        verticalPosition: "top"
      });
    }
  }
}
