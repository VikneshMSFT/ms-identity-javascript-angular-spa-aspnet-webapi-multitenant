import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import * as auth from './auth-config.json';
import { AuthCode } from "./models/authcode";
import { environment } from "src/environments/environment";

@Injectable({
    providedIn: 'root'
  })
  export class AuthService {

    //apiUri = auth.resources.authApi.resourceUri;
    apiUri = environment.authApiResourceUri;

    constructor(private http: HttpClient) { }

    postAuthCode(authCode: AuthCode)
    {
      console.log("calling zoom auth servive");
      console.log(this.apiUri);
      return this.http.post<AuthCode>(this.apiUri, authCode)
    }

    postImportToTeams()
    {
      console.log("calling zoom auth servive");
      console.log(this.apiUri);
      return this.http.get<string>(this.apiUri + "/TriggerTeamsImport");
    }

  }