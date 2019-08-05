using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/authorcollections")]
    [ApiController]
    public class AuthorCollectionsController : ControllerBase
    {
        private ILibraryRepository libraryRepository;
        public AuthorCollectionsController(ILibraryRepository _libraryRepository)
        {
            libraryRepository = _libraryRepository;
        }
        public IActionResult CreateAuthorCollection([FromBody]IEnumerable<AuthorForCreateDto> authors)
        {
            if (authors == null)
            {
                return BadRequest();
            }
            var authorsEntity = Mapper.Map<IEnumerable<Author>>(authors);
            foreach (var authorEntity in authorsEntity)
                libraryRepository.AddAuthor(authorEntity);
            if (!libraryRepository.Save())
            {
                throw new Exception();
            }
            var authorCollectionsToReturn = Mapper.Map<IEnumerable<AuthorDto>>(authorsEntity);
            var idsAsString = string.Join(",", authorCollectionsToReturn.Select(a => a.Id));
            return CreatedAtRoute("GetAuthorCollection", new { ids = idsAsString }, authorCollectionsToReturn);
        }
        [HttpGet("({ids})",Name = "GetAuthorCollection")]
        public IActionResult GetAuthorCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))]  IEnumerable<Guid> ids)
        {
            if (ids == null)
                return BadRequest();
            var authorsEntity = libraryRepository.GetAuthors(ids);
            if (ids.Count() != authorsEntity.Count())
                return NotFound();
            var authorsToReturn = Mapper.Map<IEnumerable<AuthorDto>>(authorsEntity);
            return Ok(authorsToReturn);
        }
    }
}