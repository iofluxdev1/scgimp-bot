using System.Collections.Generic;

namespace StarCitizen.Gimp.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class SpectrumThread
    {
        public string Url
        {
            get
            {
                return $"https://robertsspaceindustries.com/spectrum/community/SC/forum/{SourceThread.channel_id}/thread/{SourceThread.slug}";
            }
        }

        public static SpectrumThread FromThread(Thread thread)
        {
            return new SpectrumThread()
            {
                SourceThread = thread
            };
        }

        public Thread SourceThread { get; private set; }

        #pragma warning disable IDE1006 // Naming Styles
        
        // Generated classes from Json - http://json2csharp.com/

        public class Badge
        {
            public string name { get; set; }
            public string icon { get; set; }
            public string url { get; set; }
        }

        public class Meta
        {
            public List<Badge> badges { get; set; }
        }

        public class Presence
        {
            public string status { get; set; }
            public object info { get; set; }
            public int since { get; set; }
        }

        public class Member
        {
            public string id { get; set; }
            public string displayname { get; set; }
            public string nickname { get; set; }
            public string avatar { get; set; }
            public string signature { get; set; }
            public Meta meta { get; set; }
            public Presence presence { get; set; }
        }

        public class Votes
        {
            public int count { get; set; }
            public int voted { get; set; }
        }

        public class Badge2
        {
            public string name { get; set; }
            public string icon { get; set; }
            public string url { get; set; }
        }

        public class Meta2
        {
            public List<Badge2> badges { get; set; }
            public string signature { get; set; }
        }

        public class Member2
        {
            public string id { get; set; }
            public string displayname { get; set; }
            public string nickname { get; set; }
            public string avatar { get; set; }
            public string signature { get; set; }
            public Meta2 meta { get; set; }
        }

        public class LatestActivity
        {
            public int time_created { get; set; }
            public Member2 member { get; set; }
            public string highlight_role_id { get; set; }
        }

        public class Thumbnail
        {
            public string url { get; set; }
            public int? image_width { get; set; }
            public int? image_height { get; set; }
        }

        public class MediaPreview
        {
            public string type { get; set; }
            public Thumbnail thumbnail { get; set; }
        }

        public class Thread
        {
            public string id { get; set; }
            public int time_created { get; set; }
            public int time_modified { get; set; }
            public string channel_id { get; set; }
            public string type { get; set; }
            public string slug { get; set; }
            public string subject { get; set; }
            public bool is_locked { get; set; }
            public bool is_pinned { get; set; }
            public bool is_sinked { get; set; }
            public bool is_erased { get; set; }
            public object erased_by { get; set; }
            public string tracked_post_role_id { get; set; }
            public string content_reply_id { get; set; }
            public object label { get; set; }
            public string subscription_key { get; set; }
            public Member member { get; set; }
            public bool is_new { get; set; }
            public int new_replies_count { get; set; }
            public Votes votes { get; set; }
            public int replies_count { get; set; }
            public int views_count { get; set; }
            public string highlight_role_id { get; set; }
            public LatestActivity latest_activity { get; set; }
            public string aspect { get; set; }
            public string first_tracked_reply { get; set; }
            public MediaPreview media_preview { get; set; }
        }

        public class Data
        {
            public List<Thread> threads { get; set; }
            public int threads_count { get; set; }
            public int latest { get; set; }
            public string latest_timestamp { get; set; }
        }

        public class SpectrumThreadResponse
        {
            public int success { get; set; }
            public Data data { get; set; }
            public string code { get; set; }
            public string msg { get; set; }
        }

        public class SpectrumThreadRequest
        {
            public string channel_id { get; set; }
            public int page { get; set; }
            public string sort { get; set; }
            public object label_id { get; set; }
        }

#pragma warning restore IDE1006 // Naming Styles
    }
}
