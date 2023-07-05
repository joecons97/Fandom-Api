using FandomApi.Extensions;
using FandomApi.Model;
using FandomApi.Persistence;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

namespace FandomApi
{
    public class FandomApiClient
    {
        readonly HttpClient _httpClient;
        readonly CookieContainer _cookies;
        readonly HttpClientHandler _httpClientHandler;
        readonly ICookieStorage _cookieStorage;
        readonly bool _useCookies;

        public IApiSettings ApiSettings { get; }

        public FandomApiClient(ICookieStorage storage, IApiSettings apiSettings)
        {
            _cookieStorage = storage;
            ApiSettings = apiSettings;

            _cookies = new CookieContainer();

            if(ApiSettings.GetApiSettings().IsValid())
                _cookies.SetCookies(new Uri(ApiSettings.GetApiSettings().ApiPath), _cookieStorage.GetCookiesHeader());

            _httpClientHandler = new HttpClientHandler() { CookieContainer = _cookies };
            _httpClient = new HttpClient(_httpClientHandler);
            _useCookies = true;
        }

        string GetApiPath()
        {
            var apiSettings = ApiSettings.GetApiSettings();
            if (string.IsNullOrEmpty(apiSettings.ApiPath))
                throw new Exception("Api Path not set");

            return apiSettings.ApiPath;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var loginToken = "";
            try
            {
                loginToken = await GetLoginTokenAsync();

                var result = await LoginAsync(username, password, loginToken);
                return result;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> LoginAsync(string username, string password, string token)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("action", "login"),
                new KeyValuePair<string,string>("lgname", username),
                new KeyValuePair<string,string>("lgpassword", password),
                new KeyValuePair<string,string>("lgtoken", token),
                new KeyValuePair<string,string>("format", "json"),
            });

            var response = await _httpClient.PostAsync(GetApiPath(), formContent);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(body);

            if (json["login"]["result"].ToObject<string>() != "Success")
                return false;

            if (_useCookies)
            {
                var h = response.Headers;
                var cookies = response.Headers.FirstOrDefault(x => x.Key.ToLower() == "set-cookie").Value.ToList().FirstOrDefault();
                _cookies.SetCookies(new Uri(GetApiPath()), cookies);
                _cookieStorage.SaveCookiesHeader(cookies);
            }

            return true;
        }

        public async Task LogoutAsync()
        {
            var token = await GetCSRFToken();

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("action", "logout"),
                new KeyValuePair<string,string>("format", "json"),
                new KeyValuePair<string,string>("token", token),
            });

            var response = await _httpClient.PostAsync(GetApiPath(), formContent);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetCSRFToken()
        {
            var response = await _httpClient.GetStringAsync(GetApiPath() + "?action=query&meta=tokens&format=json");
            var json = JObject.Parse(response);

            var token = json["query"]["tokens"]["csrftoken"].ToObject<string>();
            return token;
        }

        public async Task<string> GetLoginTokenAsync()
        {
            var response = await _httpClient.GetStringAsync(GetApiPath() + "?action=query&meta=tokens&type=login&format=json");
            var json = JObject.Parse(response);

            var token = json["query"]["tokens"]["logintoken"].ToObject<string>();
            return token;
        }

        public async Task<bool> UploadFileAsync(UploadFileDto dto)
        {
            using var content = new MultipartFormDataContent()
            {
                { new StreamContent(dto.File), "file", dto.FileName},
                { new StringContent(dto.Action), "action" },
                { new StringContent(dto.Format), "format" },
                { new StringContent(dto.bot.ToString()), "bot" },
                { new StringContent(dto.IgnoreWarnings.ToString()), "ignorewarnings" },
                { new StringContent(dto.FileName), "filename" },
                { new StringContent(dto.Token), "token" },
            };

            var response = await _httpClient.PostAsync(GetApiPath(), content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseBody);

            try
            {
                if (json["upload"]["result"].ToObject<string>() != "Success")
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> EditAsync(EditDto dto)
        {
            var content = new FormUrlEncodedContent(dto.ToKeyValue());
            var response = await _httpClient.PostAsync(GetApiPath(), content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseBody);

            try
            {
                if (json["edit"]["result"].ToObject<string>() != "Success")
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<UserInfo> GetUserInfoAsync()
        {
            var response = await _httpClient.GetAsync(GetApiPath() + "?action=query&meta=userinfo&format=json");
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseBody);
            var query = json["query"]["userinfo"].ToString();
            var userInfo = JsonConvert.DeserializeObject<UserInfo>(query);

            return userInfo;
        }
    }
}
