using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bav
{
    using Microsoft.Build.Framework;
    using static Path;
    using static String;

    // ReSharper disable once UnusedMember.Global
    public partial class BumpVersion
    {
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string ProjectFullPath { get; set; }

        private string ProjectFilename => IsNullOrEmpty(ProjectFullPath) ? Empty : GetFileName(ProjectFullPath);

        private static IEnumerable<ITaskItem> DefaultTaskItems
        {
            get { yield break; }
        }

        private ITaskItem[] _bumps = DefaultTaskItems.ToArray();

        // ReSharper disable once ConvertToAutoProperty
        /// <summary>
        /// Gets the Required Bumps items.
        /// </summary>
        /// <see cref="ITaskItem"/>
        [Required]
        public ITaskItem[] Bumps
        {
            get => _bumps;
            set => _bumps = value ?? DefaultTaskItems.ToArray();
        }

        private IEnumerable<IBumpVersionDescriptor> _bumpDescriptors;

        /// <summary>
        /// Gets the Most Recently specified <see cref="IBumpVersionDescriptor"/> instances
        /// according to their <see cref="IBumpVersionDescriptor.BumpPath"/>.
        /// </summary>
        /// <see cref="IBumpVersionDescriptor.BumpPath"/>
        /// <remarks>We do this in lieu of `ItemGroup´ items Include, followed by Remove, then
        /// the desired Include.</remarks>
        private IEnumerable<IBumpVersionDescriptor> BumpDescriptors
        {
            get
            {
                // Returns the set of most-recently specified Descriptors in Reverse Order.
                IEnumerable<IBumpVersionDescriptor> GetMostRecentDescriptorSpecs(IEnumerable<ITaskItem> bumps)
                {
                    // Keeps track of the Descriptors that have been returned thus far.
                    var specified = new Dictionary<string, bool>();

                    // Responds with only the Most Recently Specified Descriptors.
                    foreach (var descriptor in bumps.Select(x => x.ToDescriptor()).Reverse().Where(x =>
                    {
                        // Filter the ones that have not yet been filtered.
                        var shouldReturn = !specified.ContainsKey(x.BumpPath);
                        specified[x.BumpPath] = true;
                        return shouldReturn;
                    }))
                    {
                        yield return descriptor;
                    }
                }

                return _bumpDescriptors ?? (_bumpDescriptors
                           = GetMostRecentDescriptorSpecs(Bumps).Reverse().ToArray()
                       );
            }
        }

        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// 
        /// </summary>
        public string Configuration { get; set; }

        private ITaskItem[] _files = DefaultTaskItems.ToArray();

        /// <summary>
        /// 
        /// </summary>
        public ITaskItem[] Files
        {
            get => _files;
            set => _files = value ?? DefaultTaskItems.ToArray();
        }
    }
}
