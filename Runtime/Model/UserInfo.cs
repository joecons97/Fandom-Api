using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FandomApi.Model
{
    public class UserInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        public bool IsLoggedIn => Id != 0;
    }
}
