using System.Collections.Generic;

namespace FubuCsProjFile.MSBuild
{
    public interface MSBuildPropertySet
    {
        IEnumerable<MSBuildProperty> Properties { get; }
        MSBuildProperty GetProperty(string name);
        MSBuildProperty SetPropertyValue(string name, string value, bool preserveExistingCase);
        string GetPropertyValue(string name);
        bool RemoveProperty(string name);
        void RemoveAllProperties();
        void UnMerge(MSBuildPropertySet baseGrp, ISet<string> propertiesToExclude);
    }
}