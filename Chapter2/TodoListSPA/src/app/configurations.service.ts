import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Configuration } from "./models/configuration";
import * as auth from './auth-config.json';
import { environment } from "src/environments/environment";

@Injectable({
    providedIn: 'root'
  })
  export class ConfigurationsService {
    
    //apiUri = auth.resources.getConfigApi.resourceUri;
    apiUri = environment.getConfigApiResourceUri;

    constructor(private http: HttpClient) { }
    
    getConfigurations() { 
      return this.http.get<Configuration>(this.apiUri);
    }

  }