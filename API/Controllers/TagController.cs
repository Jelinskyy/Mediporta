using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")] // /api/users
public class TagController : ControllerBase
{
    private readonly DataContext _context;
    public TagController(DataContext context)
    {
        _context = context;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Tag>> GetUsers()
    {
        var tags = _context.Tags.ToList();

        return tags;
    }

    [HttpGet("{id}")]
    public ActionResult<Tag?> GetUsers(int id)
    {
        return _context.Tags.Find(id);
    }
}
