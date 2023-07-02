using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IApiSettings
{
    public ApiSettingsModel GetApiSettings();
    public void SetApiSettings(ApiSettingsModel apiSettings);
}
