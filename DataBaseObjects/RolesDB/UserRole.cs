using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataBaseObjects.RolesDB
{
    [Table("Roles")]
    public class UserRole
    {
        [Key]
        [Required]
        public string? RoleName { get; set; }
        [Required]
        public List<string>? Roles { get; set; }
    }
}
