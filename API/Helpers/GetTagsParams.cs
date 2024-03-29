namespace API.Helpers;

public class GetTagsParams
{
    /// <summary>
    /// Field by witch tags will be sorted
    /// </summary>
    public SortEnum sort { get; set; }
    /// <summary>
    /// Sorting order
    /// </summary>
    public OrderEnum order { get; set; }
}
