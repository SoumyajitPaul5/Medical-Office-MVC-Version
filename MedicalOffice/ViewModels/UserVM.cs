using System.ComponentModel.DataAnnotations;

namespace MedicalOffice.ViewModels
{
    public class UserVM
    {
        // ID of the user
        public string ID { get; set; }

        [Display(Name = "User Name")]
        // Username of the user
        public string UserName { get; set; }

        [Display(Name = "Roles")]
        // List of roles assigned to the user
        public List<string> UserRoles { get; set; }
    }
}
