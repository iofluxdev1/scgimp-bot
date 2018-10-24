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
    public class TextFileDiscordWebhookProvider : IDiscordWebhookProvider
    {
        /// <summary>
        /// 
        /// </summary>
        private string _path;

        /// <summary>
        /// 
        /// </summary>
        public TextFileDiscordWebhookProvider()
        {
            _path = Path.Combine(AppContext.BaseDirectory, "discordwebhooks.txt");

            Console.WriteLine(_path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<ScGimpDiscordWebhook>> GetDiscordWebhooksAsync()
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
                .Select(line => new ScGimpDiscordWebhook() { Url = line })
                .Distinct(new DiscordWebhookEqualityComparer());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="discordWebhook"></param>
        /// <returns></returns>
        public async Task RegisterAsync(ScGimpDiscordWebhook discordWebhook)
        {
            if (File.Exists(_path))
            {
                await File.AppendAllLinesAsync
                (
                    _path,
                    new string[] { discordWebhook.Url }
                );
            }
            else
            {
                await File.WriteAllLinesAsync
                (
                    _path,
                    new string[] { discordWebhook.Url }
                );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="discordWebhook"></param>
        /// <returns></returns>
        public async Task DeleteAsync(ScGimpDiscordWebhook discordWebhook)
        {
            string[] lines = await File.ReadAllLinesAsync(_path);

            if (File.Exists(_path))
            {
                await File.WriteAllLinesAsync
                (
                    _path,
                    lines.Where(l => discordWebhook.Url.ToLowerInvariant().Trim() != l.ToLowerInvariant().Trim())
                );
            }
        }
    }
}
