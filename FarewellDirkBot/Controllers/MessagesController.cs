using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Chronic;
using FarewellDirkBot.Data;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace FarewellDirkBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                if (this.TextIsHello(activity.Text))
                {
                    var message = activity;

                    Activity reply = message.CreateReply($"Hey Dirk, we want to whish you all the best for your upcomming challanges.");
                    await connector.Conversations.ReplyToActivityAsync(reply);

                    reply = message.CreateReply($"I am a wish box containing lots of messages from all your soon to be former coworkers. You can go through all wishes step by step, get random wishes or you type a name to get a specific wish. We hope you like it.");
                    await connector.Conversations.ReplyToActivityAsync(reply);

                    var followup = this.CreateSuccessor(activity, null);
                    await connector.Conversations.ReplyToActivityAsync(followup);
                }
                else
                {
                    Farewell f = null;
                    Activity farewell = null;

                    var words = activity.Text.Split(' ');
                    foreach (var x in words)
                    {
                        farewell = CreateFarewell(x, activity, out f);
                        if (farewell != null) break;
                    }

                    if (farewell == null) farewell = this.CreateRandomFarewell(activity, out f);
                    await connector.Conversations.ReplyToActivityAsync(farewell);

                    var followup = this.CreateSuccessor(activity, f);
                    await connector.Conversations.ReplyToActivityAsync(followup);
                }
            }
            else
            {
                await HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private bool TextIsHello(string text)
        {
            var split = text.Split(' ');
            return split.Any(x => x.ToLower().Equals("hi")) || split.Any(x => x.ToLower().Equals("hello")) ||
                   split.Any(x => x.ToLower().Equals("hey")) || split.Any(x => x.ToLower().Equals("hallo"));
        }

        private Activity CreateSuccessor(Activity activity, Farewell f)
        {
            var card = activity.CreateReply();
             
            List<CardAction> cardButtons = new List<CardAction>();

            if (f != null)
            {
                CardAction nextButton = new CardAction()
                {
                    Value = f.Next().Name,
                    Type = "imBack",
                    Title = String.Format("Next: {0}", f.Next().Name)
                };
                cardButtons.Add(nextButton);
            }

            CardAction randomButton = new CardAction()
            {
                Value = "Random",
                Type = "imBack",
                Title = "Random"
            };
            cardButtons.Add(randomButton);
            
            var buttonCard = new HeroCard();
            buttonCard.Text = f != null ? "Next, random or type a name." : "Random or type a name.";
            buttonCard.Buttons = cardButtons;

            var cardAttachment = buttonCard.ToAttachment();
            card.Attachments = new List<Microsoft.Bot.Connector.Attachment>() { cardAttachment };

            return card;
        }

        private async Task<Activity> HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }

        private Activity CreateRandomFarewell(Activity message, out Farewell farewell)
        {
            var f = Farewell.Random();

            farewell = f;
            return f.ToActivity(message);
        }

        private Activity CreateFarewell(String name, Activity message, out Farewell farewell)
        {
            Farewell.ClearCache();
            var f = Farewell.All.FirstOrDefault(x => x.Name.ToLower().Contains(name.ToLower()));
            if (f == null)
            {
                farewell = null;
                return null;
            }

            farewell = f;
            return f.ToActivity(message);
        }
    }
}