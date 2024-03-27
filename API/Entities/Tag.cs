namespace API;

public class Tag
{
    public int Id { get; set; }

    public bool HasSynonyms { get; set; }
    public bool IsMadatorOnly { get; set; }
    public bool IsRequired { get; set; }

    public int Count { get; set; }

    public string? Name { get ; set; }
}
