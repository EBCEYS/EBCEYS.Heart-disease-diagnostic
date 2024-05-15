using DataBaseObjects.UsersDB;
using HeartDiseasesDiagnosticExtentions.ResponseExtensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace DataBaseObjects.DiagnoseDB
{
    [Table("DiagnoseResults")]
    public class DiagnoseData
    {
        [Key]
        [Required]
        public string? Id { get; set; }
        [Required]
        public JsonDocument? Params { get; set; }
        [Required]
        public double? ResultValue { get; set; }
        [Required]
        public Result? Result { get; set; }
        public User? User { get; set; }
        [ForeignKey(nameof(UserId))]
        public string? UserId { get; set; }
        public string? SessionId { get; set; } = null;
    }
}
