using DataBaseObjects.AlertDB;

namespace AuthService.ApiResponses
{
    /// <summary>
    /// The user alerts response.
    /// </summary>
    public class UserAlertsResponse : BaseAuthResponse
    {
        /// <summary>
        /// The alerts.
        /// </summary>
        public List<Alert> Alerts { get; set; }
    }
}
