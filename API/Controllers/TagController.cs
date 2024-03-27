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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Tag>>> GetTags()
    {
        ActionResult<IEnumerable<Tag>> tags;

        // Check if there are tags in db
        if(_context.Tags.Count() <= 0) {
            // if no do fetch request
            tags = await FetchTags();
        }else {
            // if yes get them from db
            tags = await _context.Tags.ToListAsync();
        }
        
        return tags;
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

    [HttpGet("fetch")]
    public async Task<ActionResult<IEnumerable<Tag>>> FetchTags()
    {
        string apiUrl = "https://api.stackexchange.com/2.3/tags?order=desc&sort=popular&site=stackoverflow&pagesize=100"; 

        //page number
        int pageNumber = 1;

        //all tags list
        List<Tag> tags = new List<Tag>();

        do{
            // Send request to api
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl+"&page="+pageNumber);
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

        await _context.Tags.AddRangeAsync(tags);
        await _context.SaveChangesAsync();
        
        return Ok(tags);
    }
}
