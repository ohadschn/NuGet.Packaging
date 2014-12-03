﻿using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NuGet.Frameworks
{
    public class NuGetFramework : IEquatable<NuGetFramework>
    {
        private readonly string _frameworkName;
        private readonly Version _version;
        private readonly string _profile;
        private const string _portable = "portable";
        private readonly static Version _emptyVersion = new Version(0, 0);

        public NuGetFramework(string framework)
            : this(framework, _emptyVersion)
        {

        }

        public NuGetFramework(string framework, Version version)
            : this(framework, version, null)
        {

        }

        public NuGetFramework(string framework, Version version, string profile)
        {
            _frameworkName = framework;
            _version = NormalizeVersion(version);
            _profile = profile ?? string.Empty;
        }

        public string Framework
        {
            get
            {
                return _frameworkName;
            }
        }

        public Version Version
        {
            get
            {
                return _version;
            }
        }

        public string Profile
        {
            get
            {
                return _profile;
            }
        }

        public string FullFrameworkName
        {
            get
            {
                List<string> parts = new List<string>(3) { Framework };

                StringBuilder sb = new StringBuilder(String.Format(CultureInfo.InvariantCulture, "Version=v{0}.{1}", Version.Major, Version.Minor));

                if (Version.Build > 0 || Version.Revision > 0)
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, ".{0}", Version.Build);

                    if (Version.Revision > 0)
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture, ".{0}", Version.Revision);
                    }
                }

                parts.Add(sb.ToString());

                if (!String.IsNullOrEmpty(Profile))
                {
                    parts.Add(String.Format(CultureInfo.InvariantCulture, "Profile={0}", Profile));
                }

                return String.Join(", ", parts);
            }
        }

        public bool IsPCL
        {
            get
            {
                return Version.Major == 0 && StringComparer.OrdinalIgnoreCase.Equals(Framework, FrameworkConstants.FrameworkIdentifiers.Portable);
            }
        }

        public override string ToString()
        {
            return FullFrameworkName;
        }

        public bool Equals(NuGetFramework other)
        {
            return Comparer.Equals(this, other);
        }

        public static readonly NuGetFramework UnsupportedFramework = new NuGetFramework("Unsupported");
        public static readonly NuGetFramework EmptyFramework = new NuGetFramework(string.Empty);
        public static readonly NuGetFramework AnyFramework = new NuGetFramework("Any");

        /// <summary>
        /// True if this framework matches for all versions. 
        /// Ex: net
        /// </summary>
        public bool AllVersions
        {
            get
            {
                return Version.Major == 0 && Version.Minor == 0 && Version.Build == 0 && Version.Revision == 0;
            }
        }

        public bool IsUnsupported
        {
            get
            {
                return this == UnsupportedFramework;
            }
        }

        /// <summary>
        /// True if this is the EMPTY framework
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return this == EmptyFramework;
            }
        }

        /// <summary>
        /// True if this is the ANY framework
        /// </summary>
        public bool IsAny
        {
            get
            {
                return this == AnyFramework;
            }
        }

        public bool IsSpecificFramework
        {
            get
            {
                return !IsEmpty && !IsAny && !IsUnsupported;
            }
        }

        /// <summary>
        /// Creates a NuGetFramework from a folder name using the default mappings.
        /// </summary>
        public static NuGetFramework Parse(string folderName)
        {
            return Parse(folderName, DefaultFrameworkNameProvider.Instance);
        }

        /// <summary>
        /// Creates a NuGetFramework from a folder name using the given mappings.
        /// </summary>
        public static NuGetFramework Parse(string folderName, IFrameworkNameProvider mappings)
        {
            if (folderName == null)
            {
                throw new ArgumentNullException("folderName");
            }

            NuGetFramework result = UnsupportedFramework;

            Match match = FrameworkConstants.FrameworkRegex.Match(folderName);

            if (match.Success)
            {
                string framework = mappings.GetIdentifier(match.Groups["Framework"].Value);

                // TODO: support number only folder names like 45
                if (!String.IsNullOrEmpty(framework))
                {
                    Version version = mappings.GetVersion(match.Groups["Version"].Value);

                    // make sure we have a valid version or none at all
                    if (version != null)
                    {
                        string profileShort = match.Groups["Profile"].Value.TrimStart('-');
                        string profile = mappings.GetProfile(profileShort);

                        if (StringComparer.OrdinalIgnoreCase.Equals(FrameworkConstants.FrameworkIdentifiers.Portable, framework))
                        {
                            IEnumerable<NuGetFramework> clientFrameworks = mappings.GetPortableFrameworks(profileShort);

                            string portableProfileNumber = FrameworkNameHelpers.GetPortableProfileNumberString(mappings.GetPortableProfile(clientFrameworks));

                            result = new NuGetFramework(framework, version, portableProfileNumber);
                        }
                        else
                        {
                            result = new NuGetFramework(framework, version, profile);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Full framework name comparison.
        /// </summary>
        public static IEqualityComparer<NuGetFramework> Comparer
        {
            get
            {
                return new NuGetFrameworkFullComparer();
            }
        }

        /// <summary>
        /// Framework name only comparison.
        /// </summary>
        public static IEqualityComparer<NuGetFramework> FrameworkNameComparer
        {
            get
            {
                return new NuGetFrameworkNameComparer();
            }
        }

        /// <summary>
        /// Framework name only comparison.
        /// </summary>
        public static IEqualityComparer<NuGetFramework> FrameworkProfileComparer
        {
            get
            {
                return new NuGetFrameworkProfileComparer();
            }
        }

        private static Version NormalizeVersion(Version version)
        {
            return new Version(Math.Max(version.Major, 0),
                               Math.Max(version.Minor, 0),
                               Math.Max(version.Build, 0),
                               Math.Max(version.Revision, 0));
        }
    }
}