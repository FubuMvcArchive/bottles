using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using FubuCore;

namespace Bottles.Services.Remote
{
    public class AssemblyRequirement
    {
        private readonly static FileSystem fileSystem = new FileSystem();
        private readonly Assembly _assembly;
        private readonly AssemblyCopyMode _copyMode;

        public AssemblyRequirement(string name)
            : this(name, AssemblyCopyMode.Once)
        {
        }

        public AssemblyRequirement(string name, AssemblyCopyMode copyMode)
        {
            _copyMode = copyMode;
            _assembly = Assembly.Load(name);
        }

        public AssemblyRequirement(Assembly assembly)
            : this(assembly, AssemblyCopyMode.Once)
        {
        }

        public AssemblyRequirement(Assembly assembly, AssemblyCopyMode copyMode)
        {
            _copyMode = copyMode;
            _assembly = assembly;
        }

        private bool ShouldCopyFile(string fileName)
        {
            if (_copyMode == AssemblyCopyMode.Always) return true;

            return !fileSystem.FileExists(fileName);
        }

        public void Move(string directory)
        {
            var location = _assembly.Location;
            var fileName = Path.GetFileName(location);

            var filePath = directory.AppendPath(fileName);
            if (ShouldCopyFile(filePath))
            {
                fileSystem.CopyToDirectory(location, directory);
            }


            var pdb = Path.GetFileNameWithoutExtension(fileName) + ".pdb";
            var pdbPath = directory.AppendPath(Path.GetFileName(pdb));
            if (fileSystem.FileExists(pdb) && ShouldCopyFile(pdbPath))
            {
                fileSystem.CopyToDirectory(location.ParentDirectory().AppendPath(pdb), directory);
            }
        }

        public static bool IsSemVerCompatible(string source, string destination)
        {
            var sourceVersion = SemanticVersion.Parse(source);
            var destinationVersion = SemanticVersion.Parse(destination);

            if (sourceVersion.Version.Major != destinationVersion.Version.Major)
            {
                return false;
            }

            return (sourceVersion.Version.Minor <= destinationVersion.Version.Minor);

        }
    }

    [Serializable]
    public sealed class SemanticVersion : IComparable, IComparable<SemanticVersion>, IEquatable<SemanticVersion>
    {
        private static readonly Regex _semanticVersionRegex = new Regex("^(?<Version>\\d+(\\s*\\.\\s*\\d+){0,3})(?<Release>-[a-z][0-9a-z-]*)?$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
        private static readonly Regex _strictSemanticVersionRegex = new Regex("^(?<Version>\\d+(\\.\\d+){2})(?<Release>-[a-z][0-9a-z-]*)?$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
        private const RegexOptions _flags = RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled;
        private readonly string _originalString;

        public Version Version { get; private set; }

        public string SpecialVersion { get; private set; }

        static SemanticVersion()
        {
        }

        public SemanticVersion(string version)
            : this(SemanticVersion.Parse(version))
        {
            this._originalString = version;
        }

        public SemanticVersion(int major, int minor, int build, int revision)
            : this(new Version(major, minor, build, revision))
        {
        }

        public SemanticVersion(int major, int minor, int build, string specialVersion)
            : this(new Version(major, minor, build), specialVersion)
        {
        }

        public SemanticVersion(Version version)
            : this(version, string.Empty)
        {
        }

        public SemanticVersion(Version version, string specialVersion)
            : this(version, specialVersion, (string)null)
        {
        }

        private SemanticVersion(Version version, string specialVersion, string originalString)
        {
            if (version == (Version)null)
                throw new ArgumentNullException("version");
            this.Version = SemanticVersion.NormalizeVersionValue(version);
            this.SpecialVersion = specialVersion ?? string.Empty;
            this._originalString = string.IsNullOrEmpty(originalString) ? ((object)version).ToString() + (!string.IsNullOrEmpty(specialVersion) ? (string)(object)'-' + (object)specialVersion : (string)null) : originalString;
        }

        internal SemanticVersion(SemanticVersion semVer)
        {
            this._originalString = semVer.ToString();
            this.Version = semVer.Version;
            this.SpecialVersion = semVer.SpecialVersion;
        }

        public static bool operator ==(SemanticVersion version1, SemanticVersion version2)
        {
            if (object.ReferenceEquals((object)version1, (object)null))
                return object.ReferenceEquals((object)version2, (object)null);
            else
                return version1.Equals(version2);
        }

        public static bool operator !=(SemanticVersion version1, SemanticVersion version2)
        {
            return !(version1 == version2);
        }

        public static bool operator <(SemanticVersion version1, SemanticVersion version2)
        {
            if (version1 == (SemanticVersion)null)
                throw new ArgumentNullException("version1");
            else
                return version1.CompareTo(version2) < 0;
        }

        public static bool operator <=(SemanticVersion version1, SemanticVersion version2)
        {
            if (!(version1 == version2))
                return version1 < version2;
            else
                return true;
        }

        public static bool operator >(SemanticVersion version1, SemanticVersion version2)
        {
            if (version1 == (SemanticVersion)null)
                throw new ArgumentNullException("version1");
            else
                return version2 < version1;
        }

        public static bool operator >=(SemanticVersion version1, SemanticVersion version2)
        {
            if (!(version1 == version2))
                return version1 > version2;
            else
                return true;
        }

        public string[] GetOriginalVersionComponents()
        {
            if (string.IsNullOrEmpty(this._originalString))
                return SemanticVersion.SplitAndPadVersionString(((object)this.Version).ToString());
            int length = this._originalString.IndexOf('-');
            return SemanticVersion.SplitAndPadVersionString(length == -1 ? this._originalString : this._originalString.Substring(0, length));
        }

        private static string[] SplitAndPadVersionString(string version)
        {
            string[] strArray1 = version.Split(new char[1]
      {
        '.'
      });
            if (strArray1.Length == 4)
                return strArray1;
            string[] strArray2 = new string[4]
      {
        "0",
        "0",
        "0",
        "0"
      };
            Array.Copy((Array)strArray1, 0, (Array)strArray2, 0, strArray1.Length);
            return strArray2;
        }

        public static SemanticVersion Parse(string version)
        {
            SemanticVersion semanticVersion;
            TryParse(version, out semanticVersion);
            return semanticVersion;

        }

        public static bool TryParse(string version, out SemanticVersion value)
        {
            return SemanticVersion.TryParseInternal(version, SemanticVersion._semanticVersionRegex, out value);
        }

        public static bool TryParseStrict(string version, out SemanticVersion value)
        {
            return SemanticVersion.TryParseInternal(version, SemanticVersion._strictSemanticVersionRegex, out value);
        }

        private static bool TryParseInternal(string version, Regex regex, out SemanticVersion semVer)
        {
            semVer = (SemanticVersion)null;
            if (string.IsNullOrEmpty(version))
                return false;
            Match match = regex.Match(version.Trim());
            Version result;
            if (!match.Success || !Version.TryParse(match.Groups["Version"].Value, out result))
                return false;
            semVer = new SemanticVersion(SemanticVersion.NormalizeVersionValue(result), match.Groups["Release"].Value.TrimStart(new char[1]
      {
        '-'
      }), version.Replace(" ", ""));
            return true;
        }

        public static SemanticVersion ParseOptionalVersion(string version)
        {
            SemanticVersion semanticVersion;
            SemanticVersion.TryParse(version, out semanticVersion);
            return semanticVersion;
        }

        private static Version NormalizeVersionValue(Version version)
        {
            return new Version(version.Major, version.Minor, Math.Max(version.Build, 0), Math.Max(version.Revision, 0));
        }

        public int CompareTo(object obj)
        {
            if (object.ReferenceEquals(obj, (object)null))
                return 1;
            SemanticVersion other = obj as SemanticVersion;

                return this.CompareTo(other);
        }

        public int CompareTo(SemanticVersion other)
        {
            if (object.ReferenceEquals((object)other, (object)null))
                return 1;
            int num = this.Version.CompareTo(other.Version);
            if (num != 0)
                return num;
            bool flag1 = string.IsNullOrEmpty(this.SpecialVersion);
            bool flag2 = string.IsNullOrEmpty(other.SpecialVersion);
            if (flag1 && flag2)
                return 0;
            if (flag1)
                return 1;
            if (flag2)
                return -1;
            else
                return StringComparer.OrdinalIgnoreCase.Compare(this.SpecialVersion, other.SpecialVersion);
        }

        public override string ToString()
        {
            return this._originalString;
        }

        public bool Equals(SemanticVersion other)
        {
            if (!object.ReferenceEquals((object)null, (object)other) && this.Version.Equals(other.Version))
                return this.SpecialVersion.Equals(other.SpecialVersion, StringComparison.OrdinalIgnoreCase);
            else
                return false;
        }

        public override bool Equals(object obj)
        {
            SemanticVersion other = obj as SemanticVersion;
            if (!object.ReferenceEquals((object)null, (object)other))
                return this.Equals(other);
            else
                return false;
        }

        public override int GetHashCode()
        {
            int num = this.Version.GetHashCode();
            if (this.SpecialVersion != null)
                num = num * 4567 + this.SpecialVersion.GetHashCode();
            return num;
        }
    }
}