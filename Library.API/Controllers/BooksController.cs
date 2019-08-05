using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Library.API.Entities;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Library.API.Controllers
{
    [Route("api/authors/{authorId}/books")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private ILibraryRepository libraryRepository;
        ILogger<BooksController> logger;
        public BooksController(ILibraryRepository _libraryRepository, ILogger<BooksController> _logger)
        {
            libraryRepository = _libraryRepository;
            logger = _logger;
        }
        [HttpGet()]
        public IActionResult GetBooksForAuthor(Guid authorId)
        {
            if (!libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var bookForAuthorRepo = libraryRepository.GetBooksForAuthor(authorId);
            var bookforAuthor = Mapper.Map<IEnumerable<BookDto>>(bookForAuthorRepo);
            return Ok(bookforAuthor);
        }
        [HttpGet("{id}", Name = "GetBookForAuthor")]
        public IActionResult GetBookForAuthor(Guid authorId,Guid id)
        {
            if (!libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var bookForAuthorRepo = libraryRepository.GetBookForAuthor(authorId,id);
            if(bookForAuthorRepo==null)
            {
                return NotFound();
            }
            var bookforAuthor = Mapper.Map<BookDto>(bookForAuthorRepo);
            return Ok(bookforAuthor);
        }
        [HttpPost()]
        public IActionResult CreateBooksForAuthor(Guid authorId, [FromBody] CreateBookDto book)
        {
            if (book==null)
            {
                return NotFound();
            }
            if (string.Equals(book.Description, book.Description, StringComparison.InvariantCultureIgnoreCase))
                ModelState.AddModelError(nameof(CreateBookDto), "The provider description should be different from the title");
            if (!ModelState.IsValid)
            {
                return new Helpers.UnprocessableEntityObjectResult(ModelState);
            }
            if (!libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var bookEntity = Mapper.Map<Book>(book);
            libraryRepository.AddBookForAuthor(authorId, bookEntity);
            if (!libraryRepository.Save())
            {
                throw new Exception($"Creating a book for author {authorId} faild on save" ); 
            }
            var bookToReturn = Mapper.Map<BookDto>(bookEntity); 
            return CreatedAtRoute("GetBookForAuthor", new { authorId, id = bookToReturn.Id }, bookToReturn);
        }
        [HttpDelete("{id}")]
        public IActionResult DeleteBooksForAuthor(Guid authorId, Guid id)
        {
            var bookForAnAuthourFromRepo = libraryRepository.GetBookForAuthor(authorId, id);
            if(bookForAnAuthourFromRepo==null)
            {
                return NotFound();
            }
            libraryRepository.DeleteBook(bookForAnAuthourFromRepo);
            if (!libraryRepository.Save())
            {
                throw new Exception($"Deleting a book for author {authorId} faild on save");
            }
            logger.LogInformation(100, $"Book {id} for author {authorId} faild on deleted");
            return NoContent();
        }
        [HttpPut("{id}")]
        public IActionResult UpdateBookForAuthor(Guid authorId, Guid id,[FromBody]BookForUpdateDto book)
        {
            if(book ==null)
            {
                return BadRequest();
            }
            if (string.Equals(book.Description, book.Description, StringComparison.InvariantCultureIgnoreCase))
                ModelState.AddModelError(nameof(CreateBookDto), "The provider description should be different from the title");
            if (!ModelState.IsValid)
            {
                return new Helpers.UnprocessableEntityObjectResult(ModelState);
            }
            var bookForAnAuthourFromRepo = libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAnAuthourFromRepo == null)
            {
                var bookToAdd = Mapper.Map<Book>(book);
                bookToAdd.Id = id;
                libraryRepository.AddBookForAuthor(authorId, bookToAdd);
                if (!libraryRepository.Save())
                {
                    throw new Exception($"Update a book {id} for author {authorId} faild on save");
                }
                var bookToReturn = Mapper.Map<BookDto>(bookToAdd);
                return CreatedAtRoute("GetBookForAuthor", new { authorId, id = bookToReturn.Id }, bookToReturn);
            }
            Mapper.Map(book, bookForAnAuthourFromRepo);
            libraryRepository.UpdateBookForAuthor(bookForAnAuthourFromRepo);
            if (!libraryRepository.Save())
            {
                throw new Exception($"Update a book for author {authorId} faild on save");
            }
            return NoContent();
        }
        [HttpPatch("{id}")]
        public IActionResult PartialyUpdateBookForAuthor(Guid authorId, Guid id,[FromBody] JsonPatchDocument<BookForUpdateDto> patchDocument)
        {
            if(patchDocument==null)
            {
                return BadRequest();
            }
            if(!libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var bookForAuthorFromRepo = libraryRepository.GetBookForAuthor(authorId, id);
            if (bookForAuthorFromRepo==null)
            {
                var bookDto = new BookForUpdateDto();
                // patchDocument.ApplyTo(bookDto, ModelState);
                patchDocument.ApplyTo(bookDto);

                if (string.Equals(bookDto.Title, bookDto.Description, StringComparison.InvariantCultureIgnoreCase))
                    ModelState.AddModelError(nameof(CreateBookDto), "The provider description should be different from the title");
                TryValidateModel(bookDto);
                if (!ModelState.IsValid)
                {
                    return new Helpers.UnprocessableEntityObjectResult(ModelState);
                }
                var bookToAdd = Mapper.Map<Book>(bookDto);
                bookToAdd.Id = id;
                libraryRepository.AddBookForAuthor(authorId, bookToAdd);
                if (!libraryRepository.Save())
                {
                    throw new Exception($"Update a book {id} for author {authorId} faild on save");
                }
                var bookToReturn = Mapper.Map<BookDto>(bookToAdd);
                return CreatedAtRoute("GetBookForAuthor", new { authorId, id = bookToReturn.Id }, bookToReturn);
            }
            var bookToPatch = Mapper.Map<BookForUpdateDto>(bookForAuthorFromRepo);
            patchDocument.ApplyTo(bookToPatch,ModelState);
            if (string.Equals(bookToPatch.Title, bookToPatch.Description, StringComparison.InvariantCultureIgnoreCase))
                ModelState.AddModelError(nameof(CreateBookDto), "The provider description should be different from the title");
            TryValidateModel(bookToPatch);
            if (!ModelState.IsValid)
            {
                return new Helpers.UnprocessableEntityObjectResult(ModelState);
            }
            Mapper.Map(bookToPatch, bookForAuthorFromRepo);
            libraryRepository.UpdateBookForAuthor(bookForAuthorFromRepo);
            if(!libraryRepository.Save())
            {
                throw new Exception($"Update a book for author {authorId} faild on save");
            }
            return NoContent();
        }
    }
}