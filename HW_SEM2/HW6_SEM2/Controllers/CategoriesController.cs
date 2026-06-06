using EfCoreHomework.Data;
using EfCoreHomework.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EfCoreHomework.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll()
    {
        var categories = await _context.Categories
            .Select(category => new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        var category = await _context.Categories
            .Where(category => category.Id == id)
            .Select(category => new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            })
            .FirstOrDefaultAsync();

        if (category == null)
            return NotFound();

        return Ok(category);
    }
}
