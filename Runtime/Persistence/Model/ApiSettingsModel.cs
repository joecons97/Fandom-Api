using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApiSettingsModel
{
    public string ApiPath { get; set; }

    public bool IsValid() => !string.IsNullOrEmpty(ApiPath);
}
