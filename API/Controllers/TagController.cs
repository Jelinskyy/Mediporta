using System.Net;
using API.Data;
using API.Dtos;
using API.Entities;
using API.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")] // /api/users
public class TagController : ControllerBase
{
    private readonly DataContext _context;
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cashe;
    private readonly string _allTagsCasheKey;

    public TagController(DataContext context, IMemoryCache cashe, string allTagsCasheKey = "AllTags", HttpClient httpClient = null)
    {
        _context = context;
        _httpClient = httpClient ?? new HttpClient(new HttpClientHandler
            { 
                AutomaticDecompression = DecompressionMethods.GZip
            });
        _cashe = cashe;
        _allTagsCasheKey = allTagsCasheKey;
    }

    /// <summary>
    /// Returns list of all SO Tags stored in DB
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TagDto>>>GetTags([FromQuery]GetTagsParams sortParams)
    {
        if(!_cashe.TryGetValue(_allTagsCasheKey, out IEnumerable<TagDto> tags))
        {
            // Checking if there are tags in db
            if(_context.Tags.Count() <= 0) {
                ActionResult response = await FetchTags();
                var responseStatusCode= response as IStatusCodeActionResult;
                if(responseStatusCode.StatusCode != 200){
                    return response;
                }
            }

            try
            {
                // Prepareing Query
                long sumOfTags = _context.Tags.Sum(t => (long)t.Count);

                // Executeing query
                tags = await _context.Tags.Select(t => new TagDto(){
                    Name = t.Name,
                    HasSynonyms = t.HasSynonyms,
                    IsMadatorOnly = t.IsMadatorOnly,
                    IsRequired = t.IsRequired,
                    Count = t.Count,
                    Percent = Math.Round((double)t.Count / sumOfTags * 100, 3)
                }).ToListAsync();
                
                // Updating Cashe
                MemoryCacheEntryOptions casheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(45))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(300));
                    
                _cashe.Set(_allTagsCasheKey, tags, casheEntryOptions);
            }
            catch
            {
                return Problem("Database Connection failed");
            }
        }

        // Order tags
        tags = sortParams.sort switch
        {
            SortEnum.name => sortParams.order==OrderEnum.desc ? tags.OrderByDescending(t => t.Name) : tags.OrderBy(t => t.Name),
            SortEnum.percent => sortParams.order==OrderEnum.desc ? tags.OrderByDescending(t => t.Percent) : tags.OrderBy(t => t.Percent),
            _ => tags
        };

        return Ok(tags);
    }

    /// <summary>
    /// Forcing server to fetch data from SO API and store to DB
    /// </summary>
    [HttpGet("fetch")]
    public async Task<ActionResult> FetchTags()
    {
        string apiUrl = "https://api.stackexchange.com/2.3/tags?site=stackoverflow&pagesize=100"; 

        //page number
        int pageNumber = 1;

        //all tags list
        List<Tag> tags = new List<Tag>();
        HttpResponseMessage response;

        do{
            // Send request to api
            response = await _httpClient.GetAsync(apiUrl+"&page="+pageNumber);
            if (!response.IsSuccessStatusCode){
                // Handle the error condition
                return Problem("StackOwerflow Connection failed");
            }

            // Deserializing data
            var data = await response.Content.ReadAsStringAsync();
            var deserialized = JsonConvert.DeserializeObject<SoApiResponseDto>(data);

            // Adding data to table
            tags.AddRange(deserialized.tags);

            //braking loop if there is no next page
            if(deserialized.hasMore == false) 
            {
                break;
            }

            pageNumber++;
        }while (tags.Count < 1000);

        try
        {
            //clearing database
            _context.Tags.RemoveRange(_context.Tags);

            //Inserting new tags to database
            await _context.Tags.AddRangeAsync(tags);
            await _context.SaveChangesAsync();
        }
        catch
        {
            return Problem("Database Connection failed");
        }

        _cashe.Remove(_allTagsCasheKey);
        
        return Ok("Data fetched from "+apiUrl);
    }
}
