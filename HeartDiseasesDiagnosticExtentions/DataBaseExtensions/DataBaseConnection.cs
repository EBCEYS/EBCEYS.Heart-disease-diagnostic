namespace HeartDiseasesDiagnosticExtentions.DataBaseExtensions
{
    public class DataBaseConnection
    {
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
        public string Port { get; set; }
        public int DBRequestRetries { get; set; }
        public int DBRequestDelayBetweenRetries { get; set; }
        public bool DoFullDBVacuumOnStart { get; set; }
        public bool DoInitDataBaseOnStart { get; set; }

        public string ConnectionString 
        {
            get
            {
                return $"Host={Host};Username={Username};Password={Password};Database={Database}" + (String.IsNullOrEmpty(Port)? "": $";Port={Port}");
            }
        }

        /// <summary>
        /// Плохой способ проверки свойств класса на <c>null or string.empty</c>.
        /// </summary>
        public void CheckForNulls()
        {
            if (DBRequestRetries < 0 || DBRequestRetries > 10)
            {
                DBRequestRetries = 1;
            }
            if (DBRequestDelayBetweenRetries < 100 || DBRequestDelayBetweenRetries > 10000)
            {
                DBRequestDelayBetweenRetries = 100;
            }
            if (string.IsNullOrEmpty(Host))
            {
                throw new NullReferenceException(nameof(Host));
            }
            if (string.IsNullOrEmpty(Username))
            {
                throw new NullReferenceException(nameof(Username));
            }
            if (string.IsNullOrEmpty(Password))
            {
                throw new NullReferenceException(nameof(Password));
            }
            if (string.IsNullOrEmpty(Database))
            {
                throw new NullReferenceException(nameof(Database));
            }
        }
    }
}
