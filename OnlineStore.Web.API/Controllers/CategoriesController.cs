using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data.Providers.Context;
using OnlineStore.Data.Providers.Context.Entities;
using OnlineStore.Web.API.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineStore.Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : Controller
    {
        private readonly OnlineStoreContext _context;

        public CategoriesController(OnlineStoreContext context)
        {
            _context = context;
        }

        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Categories.ToListAsync());
        }

        [HttpGet("{id}/books")]
        public async Task<IActionResult> GetBooks(int id)
        {
            var result = await _context.Categories
                .Include(c => c.Books)
                .Where(c => c.Id == id)
                .ToListAsync()
                .ConfigureAwait(false);

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = "Author")]
        public async Task<IActionResult> Create([FromBody] CategoryModel model)
        {
            var category = new Category
            {
                Name = model.Name
            };

            var result = await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return Ok(result);
        }
    }
}
