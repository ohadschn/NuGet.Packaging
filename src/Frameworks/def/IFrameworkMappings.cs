﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Frameworks
{
    /// <summary>
    /// A raw list of framework mappings. These are indexed by the framework name provider and in most cases all mappings are
    /// mirrored so that the IFrameworkMappings implementation only needs to provide the minimum amount of mappings.
    /// </summary>
    public interface IFrameworkMappings
    {
        /// <summary>
        /// Synonym -> Identifier
        /// Ex: NET Framework -> .NET Framework
        /// </summary>
        IEnumerable<KeyValuePair<string, string>> IdentifierSynonyms { get; }

        /// <summary>
        /// Ex: .NET Framework -> net
        /// </summary>
        IEnumerable<KeyValuePair<string, string>> IdentifierShortNames { get; }

        /// <summary>
        /// Ex: WindowsPhone -> wp
        /// </summary>
        IEnumerable<KeyValuePair<string, string>> ProfileShortNames { get; }


        /// <summary>
        /// Ex: Windows 8.0 <-> NetCore 4.5
        /// Ex: Windows 8.1 <-> NetCore 4.5.1
        /// </summary>
        IEnumerable<KeyValuePair<NuGetFramework, NuGetFramework>> EquivalentFrameworks { get; }


        /// <summary>
        /// Framework, EquivalentProfile1, EquivalentProfile2
        /// Ex: Silverlight, WindowsPhone71, WindowsPhone
        /// </summary>
        IEnumerable<Tuple<string, string, string>> EquivalentProfiles { get; }
    }
}