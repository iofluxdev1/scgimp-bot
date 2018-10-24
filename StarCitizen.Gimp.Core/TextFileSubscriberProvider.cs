using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarCitizen.Gimp.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class TextFileSubscriberProvider : ISubscriberProvider
    {
        /// <summary>
        /// 
        /// </summary>
        private string _path;

        /// <summary>
        /// 
        /// </summary>
        public TextFileSubscriberProvider()
        {
            _path = Path.Combine(AppContext.BaseDirectory, "subscribers.txt");

            Console.WriteLine(_path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<ScGimpSubscriber>> GetSubscribersAsync()
        {
            string[] lines;

            // Load data from the persistent store.
            if (File.Exists(_path))
            {
                lines = await File.ReadAllLinesAsync(_path);
            }
            else
            {
                lines = new string[] { string.Empty };

                await File.WriteAllLinesAsync(_path, lines);
            }

            return lines
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(line => new ScGimpSubscriber() { Email = line })
                .Distinct(new SubscriberEqualityComparer());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscriber"></param>
        /// <returns></returns>
        public async Task SubscribeAsync(ScGimpSubscriber subscriber)
        {
            if (File.Exists(_path))
            {
                await File.AppendAllLinesAsync
                (
                    _path,
                    new string[] { subscriber.Email }
                );
            }
            else
            {
                await File.WriteAllLinesAsync
                (
                    _path,
                    new string[] { subscriber.Email }
                );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscriber"></param>
        /// <returns></returns>
        public async Task UnsubscribeAsync(ScGimpSubscriber subscriber)
        {
            string[] lines = await File.ReadAllLinesAsync(_path);

            if (File.Exists(_path))
            {
                await File.WriteAllLinesAsync
                (
                    _path,
                    lines.Where(l => subscriber.Email.ToLowerInvariant().Trim() != l.ToLowerInvariant().Trim())
                );
            }
        }
    }
}
