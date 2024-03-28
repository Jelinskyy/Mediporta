using System.Net;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")] // /api/users
public class TagController : ControllerBase
{
    private readonly DataContext _context;
    private readonly HttpClient _httpClient;

    public TagController(DataContext context)
    {
        _context = context;
        _httpClient = new HttpClient(new HttpClientHandler
            { 
                AutomaticDecompression = DecompressionMethods.GZip
            });
    }

    public partial class SOApiRespons{
        [JsonProperty("items")]
        public List<Tag>? tags { get; set; }
        [JsonProperty("has_more")]
        public bool hasMore {get; set; }
        [JsonProperty("quota_max")]
        public int quotaMax {get; set; }
        [JsonProperty("quota_remaining")]
        public int quotaRemaining {get; set; }
    }

    [HttpGet]
    public async Task<ActionResult> GetTags()
    {
        // Check if there are tags in db
        if(_context.Tags.Count() <= 0) {
            await FetchTags();
        }

        // ActionResult<IEnumerable<Tag>> tags = await _context.Tags.ToListAsync();

        var query = from t in _context.Tags
            select new {
                Name = t.Name,
                HasSynonyms = t.HasSynonyms,
                IsMadatorOnly = t.IsMadatorOnly,
                IsRequired = t.IsRequired,
                Count = t.Count,
                Percent = Math.Round((double)t.Count / _context.Tags.Sum(t => t.Count) * 100, 2)
            };
        
        return Ok(query.ToList());
    }

    [HttpGet("fetch")]
    public async Task<ActionResult> FetchTags()
    {
        string apiUrl = "https://api.stackexchange.com/2.3/tags?order=desc&sort=popular&site=stackoverflow&pagesize=100"; 

        //page number
        int pageNumber = 1;

        //all tags list
        List<Tag> tags = new List<Tag>();
        HttpResponseMessage response;

        do{
            // Send request to api
            response = await _httpClient.GetAsync(apiUrl+"&pageee="+pageNumber);
            if (!response.IsSuccessStatusCode){
                // Handle the error condition
                return StatusCode((int)response.StatusCode);
            }

            // Deserializing data
            var data = await response.Content.ReadAsStringAsync();
            var deserialized = JsonConvert.DeserializeObject<SOApiRespons>(data);

            // Adding data to table
            tags.AddRange(deserialized.tags);

            //braking loop if there is no next page
            if(deserialized.hasMore == false) 
            {
                break;
            }

            pageNumber++;
        }while (tags.Count < 100);

        _context.Tags.RemoveRange(_context.Tags);
        _context.Tags.AddRange(tags);
        await _context.SaveChangesAsync();
        
        return RedirectToAction("GetTag", "Tag");
    }
}
