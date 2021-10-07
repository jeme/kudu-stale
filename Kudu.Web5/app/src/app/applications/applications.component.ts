import {Component, Injectable, OnInit} from '@angular/core';
import {ApplicationsService} from "../core/services/ApplicationsService";
import {MatIconRegistry} from "@angular/material/icon";
import {DomSanitizer} from "@angular/platform-browser";

@Injectable({
  providedIn: 'root'
})
@Component({
  selector: 'kudu-applications',
  templateUrl: './applications.component.html',
  styleUrls: ['./applications.component.scss']
})
export class ApplicationsComponent implements OnInit {

  public applications: any[] = [];

  constructor(private applicationsService: ApplicationsService,
  private matIconRegistry:MatIconRegistry,
  private domSanitzer:DomSanitizer) {
    matIconRegistry.addSvgIconLiteral('web_application_2', domSanitzer.bypassSecurityTrustHtml(`<svg width="480" height="365" viewBox="0 0 127 96.573" xmlns="http://www.w3.org/2000/svg"><g transform="translate(-19.844 -120.385)"><path style="fill:#333;stroke-width:.717838;stroke-linejoin:round" d="M98.662 455A23.61 23.61 0 0 0 75 478.662v317.676A23.61 23.61 0 0 0 98.662 820h432.676A23.61 23.61 0 0 0 555 796.338V478.662A23.61 23.61 0 0 0 531.338 455zM410 480a15 15 0 0 1 15 15 15 15 0 0 1-15 15 15 15 0 0 1-15-15 15 15 0 0 1 15-15zm45 0a15 15 0 0 1 15 15 15 15 0 0 1-15 15 15 15 0 0 1-15-15 15 15 0 0 1 15-15zm45 0a15 15 0 0 1 15 15 15 15 0 0 1-15 15 15 15 0 0 1-15-15 15 15 0 0 1 15-15zM99.814 540h430.372a9.793 9.793 0 0 1 9.814 9.814v245.372a9.793 9.793 0 0 1-9.814 9.814H99.814A9.793 9.793 0 0 1 90 795.186V549.814A9.793 9.793 0 0 1 99.814 540z" transform="scale(.26458)"/><circle style="fill:#fff;stroke-width:.19;stroke-linejoin:round" cx="137.583" cy="14.552" r="3.969"/><text xml:space="preserve" style="font-style:normal;font-weight:400;font-size:41.0274px;line-height:1.25;font-family:sans-serif;fill:#333;fill-opacity:1;stroke:none;stroke-width:.213684" x="28.886" y="189.084"><tspan style="font-style:normal;font-variant:normal;font-weight:700;font-stretch:normal;font-size:41.0274px;font-family:Roboto;-inkscape-font-specification:'Roboto Bold';fill:#333;fill-opacity:1;stroke-width:.213684" x="28.886" y="189.084">WWW</tspan></text></g></svg>`));
    matIconRegistry.addSvgIconLiteral('web_application', domSanitzer.bypassSecurityTrustHtml(`<svg width="480" height="365" viewBox="0 0 127 96.573" xmlns="http://www.w3.org/2000/svg"><g transform="translate(-1.412 -1.323)"><path style="fill:#333;stroke-width:.189928;stroke-linejoin:round" d="M7.673 1.323a6.247 6.247 0 0 0-6.26 6.26v84.052a6.247 6.247 0 0 0 6.26 6.26h114.479a6.247 6.247 0 0 0 6.26-6.26V7.584a6.247 6.247 0 0 0-6.26-6.261zm.991 3.969h112.497a3.276 3.276 0 0 1 3.283 3.283v82.07a3.276 3.276 0 0 1-3.283 3.282H8.664a3.276 3.276 0 0 1-3.283-3.283V8.574a3.276 3.276 0 0 1 3.283-3.282z"/><circle style="fill:#fff;stroke-width:.19;stroke-linejoin:round" cx="137.583" cy="14.552" r="3.969"/><circle style="fill:#333;fill-opacity:1;stroke-width:.19;stroke-linejoin:round" cx="113.86" cy="14.552" r="3.969"/><circle style="fill:#333;fill-opacity:1;stroke-width:.19;stroke-linejoin:round" cx="101.954" cy="14.552" r="3.969"/><circle style="fill:#333;fill-opacity:1;stroke-width:.19;stroke-linejoin:round" cx="90.048" cy="14.552" r="3.969"/><text xml:space="preserve" style="font-style:normal;font-weight:400;font-size:41.0274px;line-height:1.25;font-family:sans-serif;fill:#333;fill-opacity:1;stroke:none;stroke-width:.213684" x="10.455" y="70.022"><tspan style="font-style:normal;font-variant:normal;font-weight:700;font-stretch:normal;font-size:41.0274px;font-family:Roboto;-inkscape-font-specification:'Roboto Bold';fill:#333;fill-opacity:1;stroke-width:.213684" x="10.455" y="70.022">WWW</tspan></text><rect style="fill:#333;fill-opacity:1;stroke-width:.19;stroke-linejoin:round" width="108.479" height="1.323" x="10.673" y="22.49" ry=".661"/></g></svg>`))
  }

  async ngOnInit() {
    const apps = await this.applicationsService.getAll();
    this.applications = <any>apps;
  }

}
