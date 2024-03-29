using Newtonsoft.Json;

namespace API.Entities;

public class Tag
{
    public int Id { get; set; }
    public string Name { get ; set; }
    [JsonProperty("has_synonyms")]
    public bool HasSynonyms { get; set; }
    [JsonProperty("is_moderator_only")]
    public bool IsMadatorOnly { get; set; }
    [JsonProperty("is_required")]
    public bool IsRequired { get; set; }

    public int Count { get; set; }

}
