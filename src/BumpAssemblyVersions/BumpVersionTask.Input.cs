namespace Bav
{
    using Microsoft.Build.Framework;

    public partial class BumpVersion
    {
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string ProjectFullPath { get; set; }

        private ITaskItem[] _bumps;

        // ReSharper disable once ConvertToAutoProperty
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public ITaskItem[] Bumps
        {
            get => _bumps;
            set => _bumps = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Configuration { get; set; }
    }
}
