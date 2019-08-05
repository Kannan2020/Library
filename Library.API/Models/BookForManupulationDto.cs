using System.ComponentModel.DataAnnotations;

namespace Library.API.Models
{
    public abstract class BookForManupulationDto
    {
        [Required(ErrorMessage ="You should fill out a title")]
        [MaxLength(100, ErrorMessage ="The title shouldn't have moew than 100 charecters")]
        public string Title { get; set; }
        [MaxLength(500, ErrorMessage = "The description shouldn't have moew than 500 charecters")]
        public virtual string Description { get; set; }
    }
}
