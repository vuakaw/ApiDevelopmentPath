using CourseLibrary.API.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.Models
{
    [CourseTitleMustBeDifferentFromDescription(
          ErrorMessage = "The provided description should be different from the title.")]
    public abstract class CourseForManipulationDto
    {
        [Required(ErrorMessage = "You should fill out a title.")]
        [MaxLength(100, ErrorMessage = "The title shouldn't have more than 100 characters.")]
        public string Title { get; set; }

        [MaxLength(1500, ErrorMessage = "The description shouldn't have more than 1500 characters.")]
        //public  abstract string Description { get; set; } //indica que el miembro descripcion debe
                                                           //ser implementado por clases que derivan
                                                           //de la clase abstracta
        public virtual string Description { get; set; } //El modificador Virtual es muy bueo cuando
                                //tu tienes una implementación en la clase base, que tenemos, pero
                                //quieres permitir overriding.
    }
}
