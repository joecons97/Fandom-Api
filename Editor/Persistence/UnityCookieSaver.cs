using FandomApi.Persistence;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UnityFandomSettings : ICookieStorage, IApiSettings
{
    public ApiSettingsModel GetApiSettings()
    {
        return new ApiSettingsModel()
        {
            ApiPath = EditorPrefs.HasKey("Fandom_Api_Path") ? EditorPrefs.GetString("Fandom_Api_Path") : null
        };
    }

    public void SetApiSettings(ApiSettingsModel apiSettings)
    {
        EditorPrefs.SetString("Fandom_Api_Path", apiSettings.ApiPath);
    }

    public void ClearCookie()
    {
        if (EditorPrefs.HasKey("Fandom_Login_Cookie"))
            EditorPrefs.DeleteKey("Fandom_Login_Cookie");
    }

    public string GetCookiesHeader()
    {
        if (EditorPrefs.HasKey("Fandom_Login_Cookie"))
            return EditorPrefs.GetString("Fandom_Login_Cookie");

        return "";
    }

    public void SaveCookiesHeader(string cookies)
    {
        EditorPrefs.SetString("Fandom_Login_Cookie", cookies);
    }
}
