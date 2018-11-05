using System.Collections.Generic;

namespace StarCitizen.Gimp.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class SpectrumThreadEqualityComparer : IEqualityComparer<SpectrumThread>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Equals(SpectrumThread x, SpectrumThread y)
        {
            return x?.SourceThread?.id == y?.SourceThread?.id;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetHashCode(SpectrumThread obj)
        {
            return obj.SourceThread.id.GetHashCode();
        }
    }
}
