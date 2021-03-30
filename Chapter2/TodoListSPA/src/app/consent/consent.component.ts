import { Component } from '@angular/core';
import { MsalService } from '@azure/msal-angular';
import { environment } from 'src/environments/environment';
import * as auth from '../auth-config.json';
import { AuthService } from '../auth.servive';

@Component({
  selector: 'app-consent',
  templateUrl: './consent.component.html',
  styleUrls: ['./consent.component.css']
})
export class ConsentComponent {
  
  constructor(private authService : AuthService) { }

  adminConsent() {   
    this.authService.postImportToTeams();         
  }
}
