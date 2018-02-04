using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    [Serializable]
    public class EchoDialog : IDialog<object>
    {
        protected int count = 1;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;

            var replyMessage = context.MakeMessage();

            Attachment attachment = null;

            if (message.Text == "reset")
            {
                PromptDialog.Confirm(
                    context,
                    AfterResetAsync,
                    "Are you sure you want to reset the count?",
                    "Didn't get that!",
                    promptStyle: PromptStyle.Auto);
            }
            else if (message.Text == "cat")
            {
                attachment = GetGiphyAttachment();

                replyMessage.Attachments = new List<Attachment> { attachment };

                await context.PostAsync(replyMessage);
            }
            else
            {
                await context.PostAsync($"{this.count++}: You sez {message.Text}, d00d");
                context.Wait(MessageReceivedAsync);
            }
        }

        public async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
        {
            var confirm = await argument;
            if (confirm)
            {
                this.count = 1;
                await context.PostAsync("Reset count.");
            }
            else
            {
                await context.PostAsync("Did not reset count.");
            }
            context.Wait(MessageReceivedAsync);
        }

        private static Attachment GetGiphyAttachment()
        {
            var giphyQueryTerm = "cat";
            var giphyGetUrl = "https://api.giphy.com/v1/gifs/search?api_key=8LwFB1dDg6y9TNKkpQfDx50FexqDqGOy&q=" + giphyQueryTerm + "&limit=3&offset=0&rating=G&lang=en";
            var catPictUrl = "";

            using (WebClient wc = new WebClient())
            {
                var responseString = wc.DownloadString(giphyGetUrl);
                GiphyData giphyJson = JsonConvert.DeserializeObject<GiphyData>(responseString);
                catPictUrl = giphyJson.data[0].images.fixed_height.url;
            }

            return new Attachment
            {
                Name = "catPict.gif",
                ContentType = "image/gif",
                //ContentUrl = "https://docs.microsoft.com/en-us/bot-framework/media/how-it-works/architecture-resize.png"
                ContentUrl = catPictUrl
            };
        }

    }

    public class GiphyData
    {
        public List<Datum> data { get; set; }
        //public Pagination pagination { get; set; }
        public Meta meta { get; set; }
    }

    public class Datum
    {
        public string type { get; set; }
        public string id { get; set; }
        public string slug { get; set; }
        public string url { get; set; }
        public string bitly_gif_url { get; set; }
        public string bitly_url { get; set; }
        public string embed_url { get; set; }
        public string username { get; set; }
        public string source { get; set; }
        public string rating { get; set; }
        public string content_url { get; set; }
        public string source_tld { get; set; }
        public string source_post_url { get; set; }
        public int is_indexable { get; set; }
        public int is_sticker { get; set; }
        public string import_datetime { get; set; }
        public string trending_datetime { get; set; }
        public Images images { get; set; }
        public string title { get; set; }
    }

    public class Meta
    {
        public int status { get; set; }
        public string msg { get; set; }
        public string response_id { get; set; }
    }

    public class Images
    {
        public FixedHeightStill fixed_height_still { get; set; }
        //public OriginalStill original_still { get; set; }
        //public FixedWidth fixed_width { get; set; }
        //public FixedHeightSmallStill fixed_height_small_still { get; set; }
        //public FixedHeightDownsampled fixed_height_downsampled { get; set; }
        //public Preview preview { get; set; }
        //public FixedHeightSmall fixed_height_small { get; set; }
        //public DownsizedStill downsized_still { get; set; }
        //public Downsized downsized { get; set; }
        //public DownsizedLarge downsized_large { get; set; }
        //public FixedWidthSmallStill fixed_width_small_still { get; set; }
        //public PreviewWebp preview_webp { get; set; }
        //public FixedWidthStill fixed_width_still { get; set; }
        //public __invalid_type__480wStill __invalid_name__480w_still { get; set; }
        //public FixedWidthSmall fixed_width_small { get; set; }
        //public DownsizedSmall downsized_small { get; set; }
        //public FixedWidthDownsampled fixed_width_downsampled { get; set; }
        //public DownsizedMedium downsized_medium { get; set; }
        //public Original original { get; set; }
        public FixedHeight fixed_height { get; set; }
        //public Looping looping { get; set; }
        //public OriginalMp4 original_mp4 { get; set; }
        //public PreviewGif preview_gif { get; set; }
    }

    public class FixedHeight
    {
        public string url { get; set; }
        public string width { get; set; }
        public string height { get; set; }
        public string size { get; set; }
        public string mp4 { get; set; }
        public string mp4_size { get; set; }
        public string webp { get; set; }
        public string webp_size { get; set; }
    }

    public class FixedHeightStill
    {
        public string url { get; set; }
        public string width { get; set; }
        public string height { get; set; }
        public string size { get; set; }
    }
}