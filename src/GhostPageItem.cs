using Newtonsoft.Json;

namespace GhostIntegration;

[JsonObject(MemberSerialization.OptIn)]
internal class GhostPageItem
{
    [JsonProperty(PropertyName = "slug", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Slug { get; set; }

    [JsonProperty(PropertyName = "title", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Title { get; set; }

    [JsonProperty(PropertyName = "excerpt", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Excerpt { get; set; }

    [JsonProperty(PropertyName = "html", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Html { get; set; }

    [JsonProperty(PropertyName = "status", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Status { get; set; }

    [JsonProperty(PropertyName = "visibility", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Visibility { get; set; }

    [JsonProperty(PropertyName = "created_at", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public DateTime CreatedAt { get; set; }

    [JsonProperty(PropertyName = "updated_at", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public DateTime UpdatedAt { get; set; }

    [JsonProperty(PropertyName = "published_at", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public DateTime PublishedAt { get; set; }
}
