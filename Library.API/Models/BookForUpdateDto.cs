using System.ComponentModel.DataAnnotations;

namespace Library.API.Models
{
    public class BookForUpdateDto: BookForManupulationDto
    {
        [Required(ErrorMessage = "You should fill out a description")]
        public override string Description { get => base.Description; set => base.Description = value; }
    }
}
