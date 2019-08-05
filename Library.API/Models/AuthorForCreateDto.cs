using System;
using System.Collections.Generic;

namespace Library.API.Models
{
    public class AuthorForCreateDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }
        public string Genre { get; set; }
        public ICollection<CreateBookDto> books { get; set; }
    }
}
