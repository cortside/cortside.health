using System;

namespace Cortside.Health.Models {
    public class Availability {
        public int Count { get; set; }
        public int Success { get; set; }
        public int Failure { get; set; }
        public double Uptime { get; set; }
        public long TotalDuration { get; set; }
        public double AverageDuration { get; set; }
        public DateTime LastSuccess { get; set; }
        public DateTime LastFailure { get; set; }

        public void UpdateStatistics(bool healthy, long elapsedMilliseconds) {
            Count++;

            if (healthy) {
                Success++;
                LastSuccess = DateTime.UtcNow;
            } else {
                Failure++;
                LastFailure = DateTime.UtcNow;
            }

            Uptime = Success * 100.0 / Count;
            TotalDuration += elapsedMilliseconds;
            AverageDuration = TotalDuration / Convert.ToDouble(Count);
        }
    }
}
