using System;
using Cortside.Health.Enums;
using Newtonsoft.Json;

namespace Cortside.Health.Models {
    /// <summary>
    /// Service status
    /// </summary>
    public class ServiceStatusModel {
        /// <summary>
        /// Is the service healhy or not.  Healthy service may included degraded functionality.
        /// </summary>
        public bool Healthy { get; set; }

        public ServiceStatus Status { get; set; }

        public string StatusDetail { get; set; }

        public DateTime Timestamp { get; set; }

        public bool Required { get; set; }

        public Availability Availability { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Statistics { get; set; }
    }
}
