using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
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
        public List<Tag>? tags { get; set; }
        public bool hasMore {get; set; }
        public int quotaMax {get; set; }
        public int quotaRemaining {get; set; }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Tag>>> GetTags()
    {
        string apiUrl = "https://api.stackexchange.com/2.3/tags?order=desc&sort=popular&site=stackoverflow"; 
        HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadAsStringAsync();
            List<Tag>? deserialized = JsonConvert.DeserializeObject<List<Tag>>(data);
            
            // Process the data or return it as-is
            return Ok(deserialized);
        }
        else
        {
            // Handle the error condition
            return StatusCode((int)response.StatusCode);
        }
    }
}
