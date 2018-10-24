using System;
using System.Collections.Generic;
using System.Text;

namespace StarCitizen.Gimp.Core
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class BandwidthEventArgs
    {
        /// <summary>
        ///
        /// </summary>
        public long ContentLength { get; set; }

        /// <summary>
        ///
        /// </summary>
        public long TotalContentLength { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public BandwidthEventArgs()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="totalContentLength"></param>
        /// <param name="contentLength"></param>
        public BandwidthEventArgs(long totalContentLength, long contentLength)
        {
            TotalContentLength = totalContentLength;
            ContentLength = contentLength;
        }
    }
}
