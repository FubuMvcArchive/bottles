using System;
using System.Collections.Generic;
using System.Linq;

namespace Bottles.Configuration
{
    public class BottleConfigurationException : Exception
    {
        private readonly string _provenance;
        private readonly IEnumerable<BottleConfigurationError> _errors;

        public BottleConfigurationException(string provenance, IEnumerable<BottleConfigurationError> errors)
        {
            _provenance = provenance;
            _errors = errors;
        }

        public override string Message
        {
            get { return Errors.Select(x => x.ToString()).Join(", "); }
        }

        public IEnumerable<BottleConfigurationError> Errors
        {
            get { return _errors; }
        }

        public string Provenance
        {
            get { return _provenance; }
        }

        public IEnumerable<MissingPlugin> MissingPlugins
        {
            get { return _errors.OfType<MissingPlugin>(); }
        }
    }
}