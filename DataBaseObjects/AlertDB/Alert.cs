using DataBaseObjects.UsersDB;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataBaseObjects.AlertDB
{
    public class Alert
    {
        [Key]
        [Required]
        public string? Id { get; set; }
        public string? UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
        [Required]
        [Column(TypeName = "jsonb")]
        public string? Data { get; set; }
        [Required]
        public AlertLevel? Level { get; set; }
        [Required]
        public AlertType? Type { get; set; }
        [Required]
        public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;
    }
}
