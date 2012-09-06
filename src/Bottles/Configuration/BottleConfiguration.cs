using System;
using System.Collections.Generic;
using System.Linq;

namespace Bottles.Configuration
{
    // Simple notification pattern
    public class BottleConfiguration
    {
        private readonly string _provenance;
        private readonly IList<BottleConfigurationError> _errors = new List<BottleConfigurationError>();

        public BottleConfiguration(string provenance)
        {
            _provenance = provenance;
        }

        public string Provenance
        {
            get { return _provenance; }
        }

        public void RegisterError(BottleConfigurationError error)
        {
            _errors.Add(error);
        }

        public bool IsValid()
        {
            return !_errors.Any();
        }

        public IEnumerable<BottleConfigurationError> Errors
        {
            get { return _errors; }
        }

        public IEnumerable<MissingPlugin> MissingPlugins
        {
            get { return _errors.OfType<MissingPlugin>(); }
        }
    }

    public interface BottleConfigurationError { }

    public class MissingPlugin : BottleConfigurationError
    {
        public Type PluginType { get; set; }
        public string Message { get; set; }
    }
}