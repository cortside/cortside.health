using System;
using System.Collections.Generic;

namespace Cortside.Health.Models {
    /// <summary>
    /// Health
    /// </summary>
    public class HealthModel : ServiceStatusModel {
        /// <summary>
        /// The service identifier
        /// </summary>
        public string Host { get; set; } = System.Environment.MachineName;

        /// <summary>
        /// The service identifier
        /// </summary>
        public string Service { get; set; }

        /// <summary>
        /// Build
        /// </summary>
        public BuildModel Build { get; set; }

        /// <summary>
        /// Checks
        /// </summary>
        public Dictionary<string, ServiceStatusModel> Checks { get; set; }

        /// <summary>
        /// Amount of time that the service has been running
        /// </summary>
        public TimeSpan Uptime { get; set; }
    }
}
