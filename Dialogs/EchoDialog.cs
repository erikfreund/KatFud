using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SimpleEchoBot.JSON_models;
using System.Web;

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

            IntentAndEntity messageIntentAndEntity = LuisParse(message.Text);

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
            else if (messageIntentAndEntity.intent == "Feed")
            {
                await context.PostAsync($"{this.count++}: Oh boy oh boy oh boy, I can't wait for {messageIntentAndEntity.entity}!!");
                context.Wait(MessageReceivedAsync);
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

        private static IntentAndEntity LuisParse(string userMessage)
        {
            var luisAppId = "7366ffa6-30b5-45b3-b609-a34c36c58554";
            var luisSubscriptionKey = "6db85d77a2424e72b694e01d1244508b";
            var luisGetUrl = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/" + luisAppId + "?subscription-key=" + luisSubscriptionKey + "&verbose=true&timezoneOffset=-480&q=" + userMessage;
            IntentAndEntity messageIntentAndEntity = new IntentAndEntity();

            using (WebClient wc = new WebClient())
            {
                var responseString = wc.DownloadString(luisGetUrl);
                LuisData luisJson = JsonConvert.DeserializeObject<LuisData>(responseString);
                messageIntentAndEntity.intent = luisJson.topScoringIntent.intent;
                messageIntentAndEntity.entity = luisJson.entities[0].entity;
            }

            return messageIntentAndEntity;
        }

    }

    public class IntentAndEntity
    {
        public string intent { get; set; }
        public string entity { get; set; }
    }

}