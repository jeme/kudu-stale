import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: "root"
})
export class ApplicationsService {
  constructor(private http: HttpClient) { }

  async getAll() {
    return this.http
      .get('api/v1/applications')
      .toPromise();
  }

  async get(name: string) : Promise<IApplication> {
    return this.http
      .get<IApplication>(`api/v1/applications/${name}`)
      .toPromise();
  }

  async post(application) : Promise<IApplication> {
    return this.http
      .post<IApplication>('api/v1/applications', application)
      .toPromise();
  }
}

export interface IApplication {
  name: string;
  primarySiteBinding: IBinding;
  primaryServiceBinding: IBinding;

  siteBindings: IBinding[];
  serviceBindings: IBinding[];

  repository: {
    type: 'None' | 'Git' | 'Mercurial';
    gitUrl?: string;
  }
}

export interface IBinding {
  siteType: 'Service'|'Live';
  port: number;
  scheme: 'Http'|'Https';
  ip: string;
  host: string;
  dnsName: string,
  certificate: any,
  sni: boolean
}
