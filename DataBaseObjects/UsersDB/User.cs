using DataBaseObjects.AlertDB;
using DataBaseObjects.DiagnoseDB;
using DataBaseObjects.RolesDB;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataBaseObjects.UsersDB
{
    [Table("Users")]
    public class User
    {
        [Key]
        [Required]
        public string? Id { get; set; }
        [Required]
        public string? Name { get; set; }
        public string? Email { get; set; } = null;
        [Required]
        public string? Password { get; set; }
        [Required]
        public UserRole? Role { get; set; }
        public string? Organization { get; set; } = null;
        [Required]
        public bool IsActive { get; set; } = true;
        public List<DataToStorage>? StorageData { get; set; } = null;
        public List<Alert>? Alerts { get; set; } = null;
        public List<DiagnoseData>? DiagnoseData { get; set; } = null;
    }
}