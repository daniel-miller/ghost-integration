using Newtonsoft.Json;

namespace GhostIntegration;

[JsonObject(MemberSerialization.OptIn)]
internal class GhostMemberItem
{
    [JsonProperty(PropertyName = "email", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Email { get; set; }

    [JsonProperty(PropertyName = "name", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Name { get; set; }

    [JsonProperty(PropertyName = "created_at", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public DateTimeOffset? CreatedAt { get; set; }
}
