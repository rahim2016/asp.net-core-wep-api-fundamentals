using System.ComponentModel.DataAnnotations;

namespace CityInfoAPI.Models
{
    /***
    *    Use fluent validation to validate the model for more complex validation rules
    */
    public class PointOfInterestForUpdateDto
    {
        [Required(ErrorMessage = "You should provide the name value")]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }
    }
}
