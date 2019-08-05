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
    [Route("api/authors")]
    public class AuthorsController : ControllerBase
    {
        private ILibraryRepository libraryRepository;
        private IUrlHelper urlHelper;
        private IPropertyMappingService propertyMappingService;
        private ITypeHelperService typeHelperService;
        public AuthorsController(ILibraryRepository _libraryRepository,
            IUrlHelper _urlHelper,
            IPropertyMappingService _propertyMappingService,
            ITypeHelperService _typeHelperService)
        {
            libraryRepository = _libraryRepository;
            urlHelper = _urlHelper;
            propertyMappingService = _propertyMappingService;
            typeHelperService = _typeHelperService;
        }
        private string CreateAuthorsResourceUri(
            AuthorsResourceParameters authorsResourceParameters,
            ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return urlHelper.Link("GetAuthors",
                      new
                      {
                          fields = authorsResourceParameters.Fields,
                          orderBy = authorsResourceParameters.OrderBy,
                          searchQuery = authorsResourceParameters.SearchQuery,
                          genre = authorsResourceParameters.Genre,
                          pageNumber = authorsResourceParameters.PageNumber - 1,
                          pageSize = authorsResourceParameters.PageSize
                      });
                case ResourceUriType.NextPage:
                    return urlHelper.Link("GetAuthors",
                      new
                      {
                          fields = authorsResourceParameters.Fields,
                          orderBy = authorsResourceParameters.OrderBy,
                          searchQuery = authorsResourceParameters.SearchQuery,
                          genre = authorsResourceParameters.Genre,
                          pageNumber = authorsResourceParameters.PageNumber + 1,
                          pageSize = authorsResourceParameters.PageSize
                      });

                default:
                    return urlHelper.Link("GetAuthors",
                    new
                    {
                        fields = authorsResourceParameters.Fields,
                        orderBy = authorsResourceParameters.OrderBy,
                        searchQuery = authorsResourceParameters.SearchQuery,
                        genre = authorsResourceParameters.Genre,
                        pageNumber = authorsResourceParameters.PageNumber,
                        pageSize = authorsResourceParameters.PageSize
                    });
            }
        }
        [HttpGet(Name = "GetAuthors")]
        public IActionResult GetAuthors(AuthorsResourceParameters authorsResourceParameters)
        {
            if (!propertyMappingService.ValidMappingExistsFor<AuthorDto,Author>(authorsResourceParameters.OrderBy))
            {
                return BadRequest();
            }
            if(!typeHelperService.TypeHasProperties<AuthorDto>(authorsResourceParameters.Fields))
            {
                return BadRequest();
            }
            var authorsFromRepo = libraryRepository.GetAuthors(authorsResourceParameters);
            var previousPageLink = authorsFromRepo.HasPrevious ?
               CreateAuthorsResourceUri(authorsResourceParameters,
               ResourceUriType.PreviousPage) : null;

            var nextPageLink = authorsFromRepo.HasNext ?
                CreateAuthorsResourceUri(authorsResourceParameters,
                ResourceUriType.NextPage) : null;

            var paginationMetadata = new
            {
                totalCount = authorsFromRepo.TotalCount,
                pageSize = authorsFromRepo.PageSize,
                currentPage = authorsFromRepo.CurrentPage,
                totalPages = authorsFromRepo.TotalPages,
                previousPageLink = previousPageLink,
                nextPageLink = nextPageLink
            };
            Response.Headers.Add("X-Pagination",
               Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));
            var authors = Mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo);
            return Ok(authors.ShapeData(authorsResourceParameters.Fields));
        }
        [HttpGet("{id}",Name = "GetAuthor")]
        public IActionResult GetAuthor([FromRoute]Guid id,[FromQuery] string fields)
        {
            if (!typeHelperService.TypeHasProperties<AuthorDto>(fields))
            {
                return BadRequest();
            }
            var authersFromRepo = libraryRepository.GetAuthor(id);
            if (authersFromRepo == null)
                return NotFound();
            var authors = Mapper.Map<AuthorDto>(authersFromRepo);
            return new JsonResult(authors.ShapeData(fields));
        }
        [HttpPost()]
        public IActionResult CreateAuthor([FromBody]AuthorForCreateDto author)
        {
            if(author==null)
            {
                return BadRequest();
            }
            var authorEntity = Mapper.Map<Author>(author);
            libraryRepository.AddAuthor(authorEntity);
            if(!libraryRepository.Save())
            {
                throw new Exception();
            }
            var authorToReturn = Mapper.Map<AuthorDto>(authorEntity);
            return CreatedAtRoute("GetAuthor", new { id = authorToReturn.Id }, authorToReturn);
        }
        [HttpPost("{id}")]
        public IActionResult BlockAuthorCreation(Guid id)
        {
            if (libraryRepository.AuthorExists(id))
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            return NotFound();
        }
        [HttpDelete("{authorId}")]
        public IActionResult DeleteAuthor(Guid authorId)
        {
            var authorFromRepo = libraryRepository.GetAuthor(authorId);
            if (authorFromRepo == null)
                return NotFound();
            libraryRepository.DeleteAuthor(authorFromRepo);
            if(!libraryRepository.Save())
            {
                throw new Exception($"Deleting author {authorId} faild on save");
            }
            return NoContent();
        }
        
    }
}