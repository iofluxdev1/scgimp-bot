using System;
using System.Linq;
using System.Xml.Linq;

namespace StarCitizen.Gimp.Core
{
    /// <summary>
    /// 
    /// </summary>
    public enum FeedItemType
    {
        Sale,
        ThisWeekInStarCitizen,
        Event,
        Portfolio,
        CallingAllDevs,
        ReverseTheVerse,
        AroundTheVerse,
        QuestionsAndAnswers,
        GalacticGuide,
        Subscriber,
        LoremakersGuideToTheGalaxy,
        MonthlyStudioReport,
        Bugsmashers,
        Unknown
    }

    /// <summary>
    /// 
    /// </summary>
    public class FeedItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime PublishDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public XElement Element { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public FeedItemType Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public FeedItem()
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        public FeedItem(XElement element)
        {
            Content = element.Elements().First(i => string.Compare(i.Name.LocalName, "description", true) == 0).Value;
            Link = element.Elements().First(i => string.Compare(i.Name.LocalName, "link", true) == 0).Value;
            PublishDate = ParseDate(element.Elements().First(i => string.Compare(i.Name.LocalName, "pubDate", true) == 0).Value);
            Title = element.Elements().First(i => string.Compare(i.Name.LocalName, "title", true) == 0).Value;
            Element = element;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private DateTime ParseDate(string date)
        {
            if (!DateTime.TryParse(date, out DateTime result))
            {
                result = DateTime.MinValue;
            }

            return result;
        }
    }
}
