using System;
using System.Collections.Generic;
using System.Text;

namespace Cortside.Health.Models {
    /// <summary>
    /// Maps the custom validation response
    /// </summary>
    public class HealthValidationModel {
        /// <summary>
        /// Is Successful
        /// </summary>
        public bool IsSuccessful { get; set; }
        /// <summary>
        /// Error Message
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
