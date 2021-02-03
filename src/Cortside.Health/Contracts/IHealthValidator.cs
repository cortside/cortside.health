using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Cortside.Health.Models;

namespace Cortside.Health.Contracts {
    /// <summary>
    /// This interface needs to be implemented in order to
    /// use a custom check implementation
    /// </summary>
    public interface IHealthValidator {
        Task<HealthValidationModel> ValidateStatus();
    }
}
