using System.Collections.Generic;

namespace StarCitizen.Gimp.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class SpectrumFeedUpdateEventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<SpectrumThread> Items { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public SpectrumFeedUpdateEventArgs()
        {
            Items = new SpectrumThread[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        public SpectrumFeedUpdateEventArgs(IEnumerable<SpectrumThread> items)
        {
            Items = items ?? new SpectrumThread[0];
        }
    }
}
