using System;

namespace Cortside.Health.Recorders {
    public class NullRecorder : IAvailabilityRecorder {

        public void RecordAvailability(string service, TimeSpan duration, bool healthy, string message) {
            // don't do a thing
        }
    }
}
