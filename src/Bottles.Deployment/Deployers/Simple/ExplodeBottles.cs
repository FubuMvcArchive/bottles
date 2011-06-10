using System;

namespace Bottles.Deployment.Deployers.Simple
{
    public class ExplodeBottles : IDirective
    {
        public ExplodeBottles()
        {
            WebContentDirectory = string.Empty;
            BinDirectory = "bin";
        }

        public string RootDirectory { get; set; }
        public string WebContentDirectory { get; set; }
        public string DataDirectory { get; set; }
        public string BinDirectory { get; set; }
    }

    
}