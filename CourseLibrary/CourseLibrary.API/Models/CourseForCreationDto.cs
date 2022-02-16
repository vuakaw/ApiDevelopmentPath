using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CourseLibrary.API.ValidationAttributes;

namespace CourseLibrary.API.Models
{
    public class CourseForCreationDto : CourseForManipulationDto //: IValidatableObject
    {
        /*public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if(Title == Description)
            {
                yield return new ValidationResult( //objeto que es usado para proveer mensajes de errores y una propiedad relacionada a esta validacion
                    "The provided description should be different from the title.",
                    new[] { "CourseForCreationDto" });
            }
        }*/
    }
}
