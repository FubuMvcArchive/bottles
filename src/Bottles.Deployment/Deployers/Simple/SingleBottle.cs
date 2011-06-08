using System;

namespace Bottles.Deployment.Deployers.Simple
{
    public class SingleBottle : IDirective
    {
        public SingleBottle()
        {
            WebContentDirectory = string.Empty;
            BinDirectory = "bin";
        }

        public string RootDirectory { get; set; }
        public string WebContentDirectory { get; set; }
        public string BinDirectory { get; set; }
        public string BottleName { get; set; }
    }
}