using API.Entities;
using Newtonsoft.Json;

namespace API.Dtos;

public class SoApiResponseDto
{
    [JsonProperty("items")]
    public List<Tag>? tags { get; set; }
    [JsonProperty("has_more")]
    public bool hasMore {get; set; }
    [JsonProperty("quota_max")]
    public int quotaMax {get; set; }
    [JsonProperty("quota_remaining")]
    public int quotaRemaining {get; set; }
}
