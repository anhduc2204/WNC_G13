using System.ComponentModel.DataAnnotations;
using WNC_G13.Models;

namespace WNC_G13.Models.ViewModels
{
    public class RegisterViewModel
    {

      // public RegisterViewModel(){
      //   User = new User();
      // }

      [Required(ErrorMessage = "Không được để trống")]
      public string FullName { get; set; }

      [Required(ErrorMessage = "Không được để trống")]
      public string Email { get; set; }

      [Required(ErrorMessage = "Không được để trống")]
      public string Password { get; set; }
      public DateTime CreatedAt { get; set; }
      public string? PhoneNumber { get; set; }
      public DateTime? Birthday { get; set; }
      public string? Address { get; set; }
      public int Role { get; set; }
      public string? OrganizationName {get; set;}
    }
}
