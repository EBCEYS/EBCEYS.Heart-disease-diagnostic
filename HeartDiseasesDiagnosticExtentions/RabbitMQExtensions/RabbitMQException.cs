namespace HeartDiseasesDiagnosticExtentions.RabbitMQExtensions
{
    /// <summary>
    /// The rabbitMQ exceptions.
    /// </summary>
    public class RabbitMQException : Exception
    {
        /// <summary>
        /// Base rabbitmq exception call.
        /// </summary>
        public RabbitMQException() : base() { }
        /// <summary>
        /// Rabbitmq exception call with message.
        /// </summary>
        /// <param name="message">The message.</param>
        public RabbitMQException(string message) : base(message) { }
    }
}
