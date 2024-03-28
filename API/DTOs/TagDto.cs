namespace API;

public class TagDto
{
    public string? Name { get ; set; }
    public bool HasSynonyms { get; set; }
    public bool IsMadatorOnly { get; set; }
    public bool IsRequired { get; set; }

    public int Count { get; set; }

    public double Percent { get; set; }
}
