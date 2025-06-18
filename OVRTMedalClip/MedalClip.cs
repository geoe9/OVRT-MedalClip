using System;
using System.Collections.Generic;
using System.Linq;
using OVRToolkit.Modules;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;

namespace OVRTMedalClip
{
    public class MedalClip : Module
    {
        Dictionary<string, object> contents = new Dictionary<string, object>();
        private static readonly HttpClient client = new HttpClient();
        public override void Start()
        {
            Print("Started Medal Clip module");
            InitModule("Medal Clip", null);
            UpdateContent();
            client.BaseAddress = new Uri("http://localhost:12665");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("publicKey", "pub_5k8LfvRrpxOFhtjconX0vB85LS34KfsN");
        }
        public override void Update() { }
        void UpdateContent()
        {
            contents.Add("MedalClip_Header", new Header("Medal Clip"));
            contents.Add("MedalClip_Button_15", new Button("15s", null, new Action(() => SendMedalClipRequest(15))));
            contents.Add("MedalClip_Button_30", new Button("30s", null, new Action(() => SendMedalClipRequest(30))));
            contents.Add("MedalClip_Button_60", new Button("1m", null, new Action(() => SendMedalClipRequest(60))));
            contents.Add("MedalClip_Button_120", new Button("2m", null, new Action(() => SendMedalClipRequest(120))));
            contents.Add("MedalClip_Button_300", new Button("5m", null, new Action(() => SendMedalClipRequest(300))));
            SetContents(contents.Values.ToArray());
        }
        public async void SendMedalClipRequest(int duration)
        {
            var json = $"{{'eventId': 'evt_ovrt_medal_clip','eventName': 'Untitled','triggerActions': ['SaveClip'],'clipOptions': {{'duration': {duration},'alertType': 'SoundOnly'}}}}";
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/v1/event/invoke", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode && responseContent.Contains("\"errorCode\":\"INACTIVE_GAME\""))
            {
                SendNotification("Medal Clip", "Medal did not detect VRChat");
            }
            else if (response.IsSuccessStatusCode && responseContent.Contains("\"success\":true"))
            {
                SendNotification("Medal Clip", $"Saved last {duration} seconds");
            }
            else
            {
                SendNotification("Medal Clip", $"Failed to save clip");
                Print("Error: " + response.StatusCode);
            }
        }
    }
}
