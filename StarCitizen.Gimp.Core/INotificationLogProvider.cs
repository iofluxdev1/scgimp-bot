using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StarCitizen.Gimp.Core
{
    /// <summary>
    /// 
    /// </summary>
    public interface INotificationLogProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logEntry"></param>
        /// <returns></returns>
        Task WriteAsync(ScGimpNotification logEntry);
    }
}
