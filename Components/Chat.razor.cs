
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using Radzen;
using System.Text;
using System.Text.Json;

namespace BlazorChatBot.Components
{
    public partial class Chat
    {
        private Message chatinput = new();
        public List<Message> _messages = new();
        private HttpClient _httpClient;

        [Inject]
        IHttpClientFactory ClientFactory { get; set; }
        [Inject]
        TooltipService tooltipService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _httpClient = ClientFactory.CreateClient();
        }

        private async Task ClearChat()
        {
            _messages = new List<Message>();
        }
        public async Task SendMessages ()
        {
            string url = "https://chatbot.khmerw.com/chat/webhooks/rest/webhook";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Accept", "application/json");
            using StringContent jsonContent = new(
                JsonSerializer.Serialize(
                    new
                    {
                        sender = "User123",
                        message = chatinput.text,
                    }),
                Encoding.UTF8,
                "application/json"
                );

            var response = await _httpClient.PostAsync(url, jsonContent);
            if (response.IsSuccessStatusCode)
            {
                var responseStream = await response.Content.ReadAsStreamAsync();
                var data = await JsonSerializer.DeserializeAsync
                    <IEnumerable<Message>>(responseStream);
                foreach (var item in data)
                {
                    
                    if(chatinput.recipient_id == "User123")
                    {
                        _messages.Add(new Message()
                        {
                            recipient_id = "User123",
                            text = chatinput.text,
                        });
                    }
                    if(item.recipient_id == "User123")
                    {
                        _messages.Add(new Message()
                        {
                            recipient_id = "User",
                            text = item.text,
                        });
                    }
                        
                }
                chatinput.text = "";
            }
            else
            {
                string err = await response.Content.ReadAsStringAsync();
            }
        }


        private void updateTextArea(ChangeEventArgs e)
        {
            chatinput.text = e.Value!.ToString()!;
            chatinput.recipient_id = "User123";
        }

        public class Message
        {
            public string recipient_id {  get; set; }
            public string text { get; set; }
            public bool IsResquet { get; set; } = true;
        }


        void ShowTooltip(ElementReference elementReference, TooltipOptions options = null) => tooltipService.Open(elementReference, "Clear Of Chat", options);
    }
}
