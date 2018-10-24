using System;
using System.Linq;
using System.Xml.Linq;

namespace StarCitizen.Gimp.Core
{
    /// <summary>
    /// 
    /// </summary>
    public enum StoreItemType
    {
        Unknown
    }

    /// <summary>
    /// 
    /// </summary>
    public class StoreItem
    {
        /// <summary>
        /// 
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public StoreItemType Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Sku { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public XElement Element { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public StoreItem()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        public StoreItem(XElement element)
        {
            Element = element;

            Sku = Element
                .Descendants()
                .Elements()
                .FirstOrDefault
                (
                    i => i
                        .Attributes(XName.Get("data-sku"))
                        .Any()
                )
                .Attribute(XName.Get("data-sku"))
                .Value;

            string costInCents = Element
                .Descendants()
                .Elements()
                .FirstOrDefault
                (
                    i => i
                        .Attributes(XName.Get("data-value"))
                        .Any()
                )
                .Attribute(XName.Get("data-value"))
                .Value;
            Cost = int.Parse(costInCents) / 100m;

            Currency = Element
                .Descendants()
                .Elements()
                .FirstOrDefault
                (
                    i => i
                        .Attributes(XName.Get("data-currency"))
                        .Any()
                )
                .Attribute(XName.Get("data-currency"))
                .Value;

            Type = StoreItemType.Unknown;

            string[] titleParts = Element
                .Descendants()
                .Elements()
                .FirstOrDefault
                (
                    i => i
                        .Attributes(XName.Get("class"))
                        .Any(a => a.Value == "title")
                )
                .Value
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty)
                .Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            Title = string.Join(" ", titleParts);

            string relativeUrl = Element
                .Descendants()
                .Elements()
                .FirstOrDefault
                (
                    i => i
                        .Attributes(XName.Get("class"))
                        .Any(a => a.Value == "more")
                )
                .Attribute(XName.Get("href"))
                .Value;

            Url = $"https://www.robertsspaceindustries.com{relativeUrl}";
        }
    }
}
