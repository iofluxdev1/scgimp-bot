using System;
using System.Collections.Generic;
using System.Text;

namespace StarCitizen.Gimp.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class StoreFeedPollEventArgs : BandwidthEventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public StoreFeedPollEventArgs() :
            base()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="totalContentLength"></param>
        /// <param name="contentLength"></param>
        public StoreFeedPollEventArgs(long totalContentLength, long contentLength) :
            base(totalContentLength, contentLength)
        {

        }
    }
}
