using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnlineStore.Data.Providers.Context;
using OnlineStore.Data.Providers.Context.Entities;
using OnlineStore.Web.API.Models;
using OnlineStore.Web.API.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;

namespace OnlineStore.Web.API.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly OnlineStoreContext _context;

        public BooksController(ILogger<UsersController> logger, OnlineStoreContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        [Authorize]
        [MapToApiVersion("1.0")]
        [MapToApiVersion("2.0")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Books.ToListAsync());
        }


        /// <summary>
        /// Gets book by Id
        /// </summary>
        /// <returns>Book entity</returns>
        /// <response code="200">Returns book</response>
        /// <response code="404">If book doesn't exist</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBook(int id)
        {
            var book = await _context.Books.Include(b => b.Categories).FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            return Ok(book);
        }


        /// <summary>
        /// Creates a new Book.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST api/books/
        ///     {
        ///        "Name": "Book 4",
        ///        "Categories": [1, 2]
        ///     }
        ///
        /// </remarks>
        /// <param name="model"></param>
        /// <returns>Created book</returns>
        /// <response code="201">Returns the created book</response>
        /// <response code="400">If model validation failed</response>
        /// <response code="401">If user is not authorized</response>
        [HttpPost]
        [Authorize]
        [SwaggerResponse(StatusCodes.Status201Created, "Book created", typeof(Book))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Book model validation failed", typeof(ValidationError))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authorized", typeof(IActionResult))]
        public async Task<IActionResult> Create([FromBody] BookModel model)
        {
            var erros = Validate(model);
            if (erros.Any())
            {
                return BadRequest(erros);
            }

            var newBook = new Book
            {
                Name = model.Name,
                Categories = _context.Categories.Where(x => model.Categories.Contains(x.Id)).ToList()
            };

            var resultTask = _context.Books.AddAsync(newBook);

            var result = await resultTask;
            await _context.SaveChangesAsync();

            return StatusCode(201, result.Entity);
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateBook([FromBody] BookModel model)
        {
            var erros = Validate(model);
            if (erros.Any())
            {
                return BadRequest(erros);
            }
            try
            {
                var book = await _context.Books.Include(b => b.Categories).FirstOrDefaultAsync(b => b.Id == model.Id);
                if (book != null)
                {
                    book.Name = model.Name;
                    book.Categories = _context.Categories.Where(x => model.Categories.Contains(x.Id)).ToList();
                }
                else
                {
                    book = new Book
                    {
                        Name = model.Name,
                        Categories = _context.Categories.Where(x => model.Categories.Contains(x.Id)).ToList()
                    };

                    var result = await _context.Books.AddAsync(book);
                    book = result.Entity;
                }

                await _context.SaveChangesAsync();

                return Ok(book);
            }
            catch (Exception)
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        [MapToApiVersion("2.0")]
        [HttpPatch("{id}/categories")]
        public async Task<IActionResult> UpdateCategories(int id, [FromBody] int[] categoryIds)
        {
            var book = await _context.Books.Include(b => b.Categories).FirstOrDefaultAsync(b => b.Id == id);
            book.Categories = _context.Categories.Where(x => categoryIds.Contains(x.Id)).ToList();

            await _context.SaveChangesAsync();

            return Ok(book);
        }

        private static IEnumerable<ValidationError> Validate(BookModel book)
        {
            var errors = new List<ValidationError>();
            if (book.Name.Length > 20)
            {
                errors.Add(new ValidationError
                {
                    Field = nameof(book.Name),
                    Message = "Book name should not be longer than 20 characters"
                });
            }

            return errors;
        }
    }
}
