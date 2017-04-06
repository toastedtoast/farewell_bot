using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace FarewellDirkBot.Data
{
    public class Farewell
    {
        public static Farewell Random()
        {
            var r = new Random();
            var index = r.Next(0, All.Count);

            Debug.WriteLine(index);

            return All[index];
        }

        private static List<Farewell> all;
        public static List<Farewell> All
        {
            get
            {
                if (all == null)
                {
                    //Dude, Microsoft!! Ich wünsche dir eine wirklich Spannende neue Zeit. Ich habe unheimlich viel von dir gelernt und bin immer wieder überrascht wie souverän du mit neuen Themen und Technologien umgehst. Jetzt nimmst du dein neues Lieblingsspielzeug und zeigst es Deutschland. Ich wünsche dir viel Spass dabei.

                    // var path = System.Web.Hosting.HostingEnvironment.MapPath("~/data.json");
                    using (WebClient wc = new WebClient() {Encoding = Encoding.UTF8})
                    {
                        var path = "https://raw.githubusercontent.com/toastedtoast/farewell/master/data/data.json";
                        var json = wc.DownloadString(path);

                        var fs = JsonConvert.DeserializeObject<List<Farewell>>(json);
                        all = fs;
                    }

                }

                return all;
            }
        }

        public string Name { get; set; }

        public string Message { get; set; }

        public Attachment[] Attachments { get; set; }
        
        public Microsoft.Bot.Connector.Activity ToActivity(Microsoft.Bot.Connector.Activity origin)
        {
            var a = origin.CreateReply(text: String.Format("{0}: " + "{1}", this.Name, this.Message));

            if (this.Attachments != null)
            {
                foreach (var attachment in this.Attachments)
                {
                    if (a.Attachments == null) a.Attachments = new List<Microsoft.Bot.Connector.Attachment>();
                    a.Attachments.Add(new Microsoft.Bot.Connector.Attachment()
                    {
                        ContentType = attachment.Type,
                        ContentUrl = attachment.Url
                    });
                }
            }

            return a;
        }

        public Farewell Next()
        {
            var obj = All.First(x => x.Name.Equals(this.Name));
            var nextIndex = All.IndexOf(obj);
            nextIndex += 1;

            if (nextIndex > All.Count - 1) nextIndex = 0;

            return All[nextIndex];
        }

        public static void ClearCache()
        {
            all = null;
        }
    }

    public class Attachment
    {
        public string Type { get; set; }

        public string Url { get; set; }
    }
}