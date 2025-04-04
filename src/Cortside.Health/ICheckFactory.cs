using System;
using Cortside.Health.Checks;
using Microsoft.Extensions.Logging;

namespace Cortside.Health {
    public interface ICheckFactory {

        public ILogger<Check> Logger { get; }

        public IAvailabilityRecorder Recorder { get; }

        public Check Create(CheckConfiguration check);

        public string ExpandTemplate(string template);

        public void RegisterCheck(string name, Type type);
    }
}
