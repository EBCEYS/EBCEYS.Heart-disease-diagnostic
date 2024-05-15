using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeartDiseasesDiagnosticExtentions.ResponseExtensions
{
    /// <summary>
    /// The action response.
    /// </summary>
    public class RestActionResponse : ActionResponse
    {
        /// <summary>
        /// The request id.
        /// </summary>
        public string RequestId { get; set; }
    }
    /// <summary>
    /// Method response.
    /// </summary>
    public class ActionResponse
    {
        /// <summary>
        /// Gets or sets the answer.
        /// </summary>
        /// <value>
        /// The answer.
        /// </value>
        public Result Answer { get; set; }
        /// <summary>
        /// Gets or sets the value. If answer is not <c>OK</c>, value is <c>null</c>; otherwise <c>double</c>.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public double? Value { get; set; }
        /// <summary>
        /// Converts this to object.
        /// </summary>
        /// <returns>The object.</returns>
        public object ToObject()
        {
            return new { answer = Answer, value = Value ?? null};
        }

        public ActionResponse() { }
    }

    /// <summary>
    /// Answer result.
    /// </summary>
    public enum Result
    {
        /// <summary>
        /// The OK.
        /// </summary>
        OK,
        /// <summary>
        /// The fatal error.
        /// </summary>
        ERROR,
        /// <summary>
        /// The error cause by wrong algorithm
        /// </summary>
        ERROR_WRONG_ALGORITHM,
        /// <summary>
        /// The error cause by wrong dataset.
        /// </summary>
        ERROR_WRONG_DATASET,
        /// <summary>
        /// The error cause by wrong requestid format.
        /// </summary>
        ERROR_WRONG_REQUEST_ID,
        /// <summary>
        /// The error cause by wrong connection
        /// </summary>
        ERROR_CONNECTION
    }
}
