using System.ComponentModel.DataAnnotations;

namespace HeartDiseasesDiagnosticExtentions.AuthExtensions
{
    /// <summary>
    /// The change password api model.
    /// </summary>
    public class ChangePasswordApiModel
    {
        /// <summary>
        /// The current password.
        /// </summary>
        [Required]
        public string CurrentPassword { get; set; }
        /// <summary>
        /// The new password.
        /// </summary>
        [Required]
        public string NewPassword { get; set; }
        /// <summary>
        /// The user id.
        /// </summary>
        public string? UserId { get; set; } = null;
    }
}
