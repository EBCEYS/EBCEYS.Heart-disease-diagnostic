using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeartDiseasesDiagnosticExtentions.AuthExtensions
{
    /// <summary>
    /// The update user role model.
    /// </summary>
    public class UpdateRolesModel
    {
        /// <summary>
        /// The UserId.
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// The new role.
        /// </summary>
        public string NewRole { get; set; }
    }
}
