using Newtonsoft.Json;

public class CreateChannelRequest
{
    [JsonProperty(PropertyName = "@microsoft.graph.channelCreationMode")]
    public string channelCreationMode { get; set; }

    [JsonProperty(PropertyName = "displayName")]
    public string displayName
    {
        get;
        set;
    }

    [JsonProperty(PropertyName = "description")]
    public string description { get; set; }

    [JsonProperty(PropertyName = "membershipType")]
    public string membershipType { get; set; }

    [JsonProperty(PropertyName = "createdDateTime")]
    public string createdDateTime { get; set; }
}

public class CreateTeam
{
    [JsonProperty(PropertyName = "@microsoft.graph.teamCreationMode")]
    public string teamCreationMode { get; set; }

    [JsonProperty(PropertyName = "bind")]
    public string bind { get; set; }

    [JsonProperty(PropertyName = "displayName")]
    public string displayName { get; set; }

    [JsonProperty(PropertyName = "description")]
    public string description { get; set; }

    [JsonProperty(PropertyName = "createdDateTime")]
    public string createdDateTime { get; set; }
}

public class ItemBody
{
    [JsonProperty(PropertyName = "contentType")]
    public string contentType { get; set; }

    [JsonProperty(PropertyName = "content")]
    public string content { get; set; }
}

public class User
{
    [JsonProperty(PropertyName = "id")]
    public string id { get; set; }

    [JsonProperty(PropertyName = "displayName")]
    public string displayName { get; set; }

    [JsonProperty(PropertyName = "userIdentityType")]
    public string userIdentityType { get; set; }

}

public class From
{
    [JsonProperty(PropertyName = "user")]
    public User user { get; set; }
}

public class ChatMessageRequest
{
    [JsonProperty(PropertyName = "createdDateTime")]
    public string createdDateTime { get; set; }

    [JsonProperty(PropertyName = "body")]
    public ItemBody body { get; set; }

    [JsonProperty(PropertyName = "from")]
    public From from { get; set; }

}

public class CreateChannelResponse
{
    [JsonProperty(PropertyName = "@odata.context")]
    string context;

    [JsonProperty(PropertyName = "id")]
    string id;

    [JsonProperty(PropertyName = "createdDateTime")]
    string createdDateTime;

    [JsonProperty(PropertyName = "displayName")]
    string displayName;

    [JsonProperty(PropertyName = "description")]
    string description;

}

public class AddMemberToTeam
{
    [JsonProperty(PropertyName = "@odata.type")]
    public string type { get; set; }

    [JsonProperty(PropertyName = "roles")]
    public string[] roles { get; set; }

    [JsonProperty(PropertyName = "user@odata.bind")]
    public string bind { get; set; }

}