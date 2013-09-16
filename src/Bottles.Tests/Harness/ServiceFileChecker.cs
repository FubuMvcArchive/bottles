using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bottles.Services;
using FubuCore;

namespace Bottles.Tests.Harness
{
    public class ServiceFileChecker
    {
        private readonly string _rootDirectory;
        private readonly string[] _directories;

        private readonly IList<string> _messages = new List<string>(); 

        public ServiceFileChecker(string rootDirectory, params string[] directories)
        {
            _rootDirectory = rootDirectory;
            _directories = directories;

            _directories.Each(dir => {
                new FileSystem().CleanDirectory(_rootDirectory.AppendPath(dir));
            });
        }

        private bool exists(string directory, string file)
        {
            return File.Exists(_rootDirectory.AppendPath(directory, file));
        }

        public void WaitForActivated()
        {
            Wait.Until(() => {
                return _directories.All(dir => exists(dir, "activate.txt"));
            }, timeoutInMilliseconds:15000);

            _directories.Where(dir => !exists(dir, "activate.txt"))
                .Each(dir => {
                    _messages.Add("Did not detect an activate.txt file for directory '{0}'".ToFormat(dir));
                });
        }

        public void WaitForDeactivated()
        {
            Wait.Until(() =>
            {
                return _directories.All(dir => exists(dir, "deactivate.txt"));
            }, timeoutInMilliseconds: 15000);

            _directories.Where(dir => !exists(dir, "deactivate.txt"))
                .Each(dir =>
                {
                    _messages.Add("Did not detect an deactivate.txt file for directory '{0}'".ToFormat(dir));
                });
        }

        public IList<string> Messages
        {
            get { return _messages; }
        }
    }
}