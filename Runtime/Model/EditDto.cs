using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FandomApi.Model
{
    public class EditDto
    {
        public EditDto(string title, string text, string token)
        {
            Title = title;
            Text = text;
            Token = token;
        }

        [JsonProperty("action")]
        public string Action => "edit";

        [JsonProperty("format")]
        public string Format => "json";

        [JsonProperty("bot")]
        public int bot => 1;

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

    }
}
