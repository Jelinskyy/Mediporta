using System.Net;
using API.Data;
using API.Dtos;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
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
    private readonly string allTagsCasheKey = "AllTags";

    public TagController(DataContext context, IMemoryCache cashe)
    {
        _context = context;
        _httpClient = new HttpClient(new HttpClientHandler
            { 
                AutomaticDecompression = DecompressionMethods.GZip
            });
        _cashe = cashe;
    }

    /// <summary>
    /// Returns list of all SO Tags stored in DB
    /// </summary>
    /// <param name="order">
    /// Sorting order
    /// </param>
    /// <param name="sort">
    /// Field by witch tags will be sorted
    /// </param>
    [HttpGet]
    public async Task<IEnumerable<TagDto>>GetTags(string order = "asc", string sort = "name")
    {
        sort = sort.ToLower();
        order = order.ToLower();

        if(!_cashe.TryGetValue(allTagsCasheKey, out IEnumerable<TagDto> tags))
        {
            // Checking if there are tags in db
            if(_context.Tags.Count() <= 0) {
                await FetchTags();
            }


            // Prepareing Query
            long sumOfTags = _context.Tags.Sum(t => (long)t.Count);

            var query = from t in _context.Tags
                select new TagDto{
                    Name = t.Name,
                    HasSynonyms = t.HasSynonyms,
                    IsMadatorOnly = t.IsMadatorOnly,
                    IsRequired = t.IsRequired,
                    Count = t.Count,
                    Percent = Math.Round((double)t.Count / sumOfTags * 100, 3)
                };

            // Executeing query
            tags = await query.ToListAsync();

            // Updating Cashe
            MemoryCacheEntryOptions casheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(45))
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(300));
                
            _cashe.Set(allTagsCasheKey, tags, casheEntryOptions);
        }

        // Order tags
        tags = sort switch
        {
            "name" => order=="desc" ? tags.OrderByDescending(t => t.Name) : tags.OrderBy(t => t.Name),
            "percent" => order=="desc" ? tags.OrderByDescending(t => t.Percent) : tags.OrderBy(t => t.Percent),
            _ => tags
        };

        return tags;
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
                return StatusCode((int)response.StatusCode);
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

        //clearing database
        _context.Tags.RemoveRange(_context.Tags);

        //Inserting new tags to database
        await _context.Tags.AddRangeAsync(tags);
        await _context.SaveChangesAsync();

        _cashe.Remove(allTagsCasheKey);
        
        return Ok("Data fetched from "+apiUrl);
    }
}
