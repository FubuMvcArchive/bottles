using System;
using System.Collections.Generic;
using Bottles.Deployment.Parsing;
using FubuCore.Binding;
using FubuCore.Configuration;
using StructureMap;
using System.Linq;

namespace Bottles.Deployment.Runtime
{



    public class DirectiveRunnerFactory : IDirectiveRunnerFactory
    {
        private readonly IContainer _container;
        private readonly IDirectiveTypeRegistry _types;
        private readonly IObjectResolver _resolver = ObjectResolver.Basic();

        public DirectiveRunnerFactory(IContainer container, IDirectiveTypeRegistry types)
        {
            _container = container;
            _types = types;
        }

        // Take this out of the public interface
        public IDirectiveRunner Build(IDirective directive)
        {
            return _container.ForObject(directive)
                .GetClosedTypeOf(typeof (DirectiveRunner<>))
                .As<IDirectiveRunner>();
        }

        // Pass in DeploymentPlan instead
        public IEnumerable<IDirectiveRunner> BuildRunners(DeploymentPlan plan)
        {
            return plan.Hosts.SelectMany(x => BuildRunnersFor(plan, x));
        }

        public IEnumerable<IDirectiveRunner> BuildRunnersFor(DeploymentPlan plan, HostManifest host)
        {
            foreach (var directive in BuildDirectives(plan, host, _types))
            {
                var runner = Build(directive);
                runner.Attach(host, directive);

                yield return runner;
            }
        }

        // overridden in testing classes
        public virtual IEnumerable<IDirective> BuildDirectives(DeploymentPlan plan, HostManifest host, IDirectiveTypeRegistry typeRegistry)
        {
            var provider = SettingsProvider.For(host.AllSettingsData().Union(plan.Substitutions()).ToArray());

            return host.UniqueDirectiveNames().Select(name =>
            {
                var type = typeRegistry.DirectiveTypeFor(name);

                return (IDirective)provider.SettingsFor(type);
            });
        }
    }
}