using System.ComponentModel.DataAnnotations;

namespace MedicalOffice.Models
{
    public class Province
    {
        [Display(Name = "Two Letter Province Code")]
        [Required(ErrorMessage = "You cannot leave the province code blank.")]
        [StringLength(2, ErrorMessage = "Province Code can only be two capital letters.")]
        [RegularExpression("^\\p{Lu}{2}$", ErrorMessage = "Please enter two capital letters for the province code.")]
        public string ID { get; set; }

        [Display(Name = "Province Name")]
        [Required(ErrorMessage = "You cannot leave the name of the province blank.")]
        [StringLength(50, ErrorMessage = "Province name can only be 50 characters long.")]
        public string Name { get; set; }

        public ICollection<City> Cities { get; set; } = new HashSet<City>();
    }
}

