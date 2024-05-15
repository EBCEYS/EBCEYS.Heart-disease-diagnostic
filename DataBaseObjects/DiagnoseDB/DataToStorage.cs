using DataBaseObjects.UsersDB;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace DataBaseObjects.DiagnoseDB
{
    [Table("UsersDataToStorage")]
    public class DataToStorage
    {
        [Key]
        [Required]
        public string? Id { get; set; }
        [Required]
        public JsonDocument? Data { get; set; }
        [Required]
        [ForeignKey(nameof(UserKey))]
        public string? UserKey { get; set; }
        public User? User { get; set; } = null;
    }
}
