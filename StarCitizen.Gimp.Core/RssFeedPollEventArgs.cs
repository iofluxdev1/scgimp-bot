using System;
using System.Collections.Generic;
using System.Text;

namespace StarCitizen.Gimp.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class RssFeedPollEventArgs : BandwidthEventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public RssFeedPollEventArgs() :
            base()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="totalContentLength"></param>
        /// <param name="contentLength"></param>
        public RssFeedPollEventArgs(long totalContentLength, long contentLength) :
            base(totalContentLength, contentLength)
        {

        }
    }
}
