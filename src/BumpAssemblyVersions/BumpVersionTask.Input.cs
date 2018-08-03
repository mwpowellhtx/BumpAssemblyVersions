using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bav
{
    using Microsoft.Build.Framework;
    using static Path;
    using static String;

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
        /// 
        /// </summary>
        [Required]
        public ITaskItem[] Bumps
        {
            get => _bumps;
            set => _bumps = value ?? DefaultTaskItems.ToArray();
        }

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
