import { OnInit } from "@angular/core";
import { Component } from "@angular/core";
import { environment } from "src/environments/environment";
import { ConfigurationsService } from "../configurations.service";
import { Configuration } from "../models/configuration";

@Component({
    selector: 'migration-view',
    templateUrl: './migration-view.component.html',
    styleUrls: ['./migration-view.component.css']
  })
  export class MigrationViewComponent implements OnInit {
   
  calendar: string = "";
  zoomClientId: string = "";
  apiClientId: string = "";
  teamsLoggedIn: string = "false";
  zoomLoggedIn: string = "false";
  
  constructor(private configService : ConfigurationsService){}

  ngOnInit(): void {
    this.calendar = 'outlook'
    this.fetchZoomClientId();
  }

  private fetchZoomClientId() : void
  {    
    var config = this.configService.getConfigurations().subscribe((config: Configuration) => {
      this.zoomClientId = config.zoomAppId;
      this.apiClientId = config.aadAppId;   
      sessionStorage.setItem("zoom_client_id", config.zoomAppId);
      sessionStorage.setItem("api_client_id", config.aadAppId);         
      this.zoomLoggedIn = config.zoomLoggedIn.toString();
      this.teamsLoggedIn = config.teamsLoggedIn.toString();
      console.log(config);      
    });    
  }

  public onSignInToZoomsClickEvent() : void
  {
    sessionStorage.setItem("current_login_attempt", "zoom");
    window.open(this.getZoomAuthorizationEndPoint(), '_self');
  }

  public onSignInToTeamsClickEvent() : void
  {
    sessionStorage.setItem("current_login_attempt", "graph");
    window.open(this.getAADAuthorizationEndPoint(), '_self');
  }
  
  private getZoomAuthorizationEndPoint() : string
  {
    return "https://zoom.us/oauth/authorize?response_type=code&client_id=" + this.zoomClientId + "&redirect_uri=" + environment.redirectUri
  }

  private getAADAuthorizationEndPoint() : string
  {
    return "https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id=" +
            this.apiClientId +
            "&response_type=code&redirect_uri=" + environment.redirectUri +
            "&scope=openid offline_access https://graph.microsoft.com/Calendars.Read https://graph.microsoft.com/Calendars.ReadWrite https://graph.microsoft.com/Channel.Create " +
            "https://graph.microsoft.com/Channel.ReadBasic.All https://graph.microsoft.com/ChannelMember.Read.All https://graph.microsoft.com/ChannelMember.ReadWrite.All " +
            "https://graph.microsoft.com/ChannelMessage.Send https://graph.microsoft.com/ChannelSettings.Read.All https://graph.microsoft.com/ChannelSettings.ReadWrite.All "+
            "https://graph.microsoft.com/Chat.Create https://graph.microsoft.com/Chat.ReadWrite https://graph.microsoft.com/ChatMember.Read https://graph.microsoft.com/ChatMessage.Send "+
            "https://graph.microsoft.com/Contacts.Read https://graph.microsoft.com/email https://graph.microsoft.com/profile https://graph.microsoft.com/Team.Create " +
            "https://graph.microsoft.com/Team.ReadBasic.All https://graph.microsoft.com/TeamMember.Read.All https://graph.microsoft.com/TeamMember.ReadWrite.All "+
            "https://graph.microsoft.com/TeamMember.ReadWriteNonOwnerRole.All https://graph.microsoft.com/User.Read https://graph.microsoft.com/User.Read.All" +
            "&response_mode=query";            
  }
  
}