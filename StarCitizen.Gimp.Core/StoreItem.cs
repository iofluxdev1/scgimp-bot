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

        private XElement XNodeToXElement(XNode xNode)
        {
            XElement element;

            try
            {
                element = XElement.Parse(xNode.ToString());
            }
            catch
            {
                element = null;
            }

            return element;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        public StoreItem(XElement element)
        {
            try
            {
                Element = element;

                XElement[] nodes = Element.DescendantNodes().Select(i => XNodeToXElement(i)).Where(i => i != null).ToArray();
                XElement outOfStock = nodes.FirstOrDefault(i => i.Value.Contains("out of stock"));
                XElement sku = nodes.FirstOrDefault
                (
                    i => i
                        .Attributes(XName.Get("data-sku"))
                        .Any()
                );

                if (outOfStock != null)
                {
                    throw new OutOfStockStoreItemException();
                }

                if (sku == null)
                {
                    throw new Exception($"sku is null: {element.ToString()}");
                }

                Sku = sku
                    .Attribute(XName.Get("data-sku"))
                    .Value;

                XElement cost = nodes.FirstOrDefault
                (
                    i => i
                        .Attributes(XName.Get("data-value"))
                        .Any()
                );

                if (cost == null)
                {
                    throw new Exception($"cost is null: {element.ToString()}");
                }

                string costInCents = cost
                    .Attribute(XName.Get("data-value"))
                    .Value;
                Cost = int.Parse(costInCents) / 100m;

                XElement currency = nodes.FirstOrDefault
                (
                    i => i
                        .Attributes(XName.Get("data-currency"))
                        .Any()
                );

                if (currency == null)
                {
                    throw new Exception($"currency is null {element.ToString()}");
                }

                Currency = currency
                    .Attribute(XName.Get("data-currency"))
                    .Value;

                Type = StoreItemType.Unknown;

                XElement title = nodes.FirstOrDefault
                (
                    i => i
                        .Attributes(XName.Get("class"))
                        .Any(a => a.Value == "title")
                );

                if (title == null)
                {
                    throw new Exception($"title is null: {element.ToString()}");
                }

                string[] titleParts = title
                    .Value
                    .Replace("\r", string.Empty)
                    .Replace("\n", string.Empty)
                    .Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                Title = string.Join(" ", titleParts);

                XElement url = nodes.FirstOrDefault
                (
                    i => i
                        .Attributes(XName.Get("class"))
                        .Any(a => a.Value == "more")
                );

                if (url == null)
                {
                    throw new Exception($"url is null: {element.ToString()}");
                }

                string relativeUrl = url
                    .Attribute(XName.Get("href"))
                    .Value;

                Url = $"https://www.robertsspaceindustries.com{relativeUrl}";
            }
            catch (OutOfStockStoreItemException)
            {
                throw;
            }
            catch (Exception ex)
            {

                throw new Exception("Failed to parse store item. See inner exception for more information.", ex);
            }
        }
    }
}
