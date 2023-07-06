using FandomApi;
using FandomApi.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class FandomApiWindow : EditorWindow
{
    FandomApiClient _apiClient;
    UserInfo _currentUserInfo = new UserInfo();
    string loginText;

    Type[] _services;
    IFandomService _activeService;
    bool _isRunningTask = false;
    Exception _exception;

    [MenuItem("Window/Fandom/Api Window")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        FandomApiWindow window = EditorWindow.GetWindow<FandomApiWindow>();
        window.titleContent = new GUIContent("Fandom");
        window.Show();
    }

    void CreateClient()
    {
        var unityImplementation = new UnityFandomSettings();
        _apiClient = new FandomApiClient(unityImplementation, unityImplementation);

        if (_apiClient.ApiSettings.GetApiSettings().IsValid() == false)
        {
            EditorApplication.delayCall += () =>
            {
                FandomSettingsWindow.Open(_apiClient);
                CheckLoginStatus();
            };
        }

        CheckLoginStatus();
    }

    async Task CheckLoginStatus()
    {
        if (_apiClient.ApiSettings.GetApiSettings().IsValid())
        {
            loginText = "Wait...";
            _currentUserInfo = await _apiClient.GetUserInfoAsync();
            loginText = _currentUserInfo.IsLoggedIn ? _currentUserInfo.Name : "Login...";
        }
        else
        {
            loginText = "Missing Api Path...";
        }
    }

    async Task Login(string username, string password)
    {
        loginText = "Logging in...";
        var result = await _apiClient.LoginAsync(username, password);
        await CheckLoginStatus();
    }

    async Task Logout()
    {
        loginText = "Logging out...";
        await _apiClient.LogoutAsync();
        await CheckLoginStatus();
    }

    async Task LoadServices()
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        List<Type> types = new List<Type>();
        foreach (Assembly assembly in assemblies)
        {
            types.AddRange(assembly.GetTypes().
            Where(x => !x.IsInterface && (x.GetInterfaces().Contains(typeof(IFandomService)) ||
            x.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IFandomService<>)))));
        }

        _services = types.ToArray();
    }

    void OnFocus()
    {
        LoadServices();
    }

    private void OnGUI()
    {
        if (_apiClient == null)
            CreateClient();

        RenderToolbar();

        if (_activeService != null)
        {
            RenderActiveService();
        }
        else
        {
            RenderServices();
        }
    }

    void RenderToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
        if (GUILayout.Button("Settings...", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
        {
            EditorApplication.delayCall += () =>
            {
                FandomSettingsWindow.Open(_apiClient);
                CheckLoginStatus();
            };
        }
        GUILayout.FlexibleSpace();

        GUI.enabled = _apiClient.ApiSettings.GetApiSettings().IsValid();
        if (GUILayout.Button(loginText, EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
        {
            if (_currentUserInfo.IsLoggedIn)
            {
                if (EditorUtility.DisplayDialog("Logout", "Do you want to logout of Fandom?", "Yes", "No"))
                    Logout();
            }
            else
            {
                var loginDiag = FandomLoginWindow.Open();
                if (loginDiag.ModalComplete)
                {
                    Login(loginDiag.Username, loginDiag.Password);
                }
            }
        }
        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
    }

    void RenderActiveService()
    {
        var rect = EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true), GUILayout.Height(32));
        EditorGUILayout.LabelField(_activeService.GetType().Name, FandomEditorStyles.ListViewHeader, GUILayout.ExpandWidth(true));

        var iconHeight = 12;
        var iconRect = new Rect(rect.x + 4, rect.y + (rect.height / 2) - (iconHeight / 2), iconHeight, iconHeight);
        var selectionRect = iconRect;
        selectionRect.width += 256;

        GUI.DrawTexture(iconRect, EditorGUIUtility.FindTexture("d_back@2x"));

        if (Event.current.type == EventType.MouseDown && iconRect.Contains(Event.current.mousePosition))
        {
            _activeService = null;
            Event.current.Use();
        }

        EditorGUILayout.EndHorizontal();

        if (_activeService == null)
            return;

        var parms = _activeService.GetType().GetProperty("Parameters");
        var paramsType = parms.PropertyType;
        var parmsValue = parms.GetValue(_activeService);
        if (parmsValue == null)
        {
            parmsValue = Activator.CreateInstance(paramsType);
            parms.SetValue(_activeService, parmsValue);
        }

        foreach (var param in paramsType.GetProperties())
        {
            var value = param.GetValue(parmsValue);

            if (param.PropertyType == typeof(string))
            {
                value = EditorGUILayout.TextField(param.Name, (string)value);
            }
            else if (param.PropertyType == typeof(float))
            {
                value = EditorGUILayout.FloatField(param.Name, (float)value);
            }
            else if (param.PropertyType == typeof(int))
            {
                value = EditorGUILayout.IntField(param.Name, (int)value);
            }

            param.SetValue(parmsValue, value);
        }
        var response = _activeService.GetType().GetProperty("Response");
        var responseType = response.PropertyType;

        var text = _isRunningTask
            ? "Running..."
            : "Run";

        GUI.enabled = !_isRunningTask;

        if (GUILayout.Button(text))
        {
            response.SetValue(_activeService, null);
            Task.Factory.StartNew(async () =>
            {
                _isRunningTask = true;
                try
                {
                    await _activeService.Execute(_apiClient);
                }
                catch(Exception ex)
                {
                    _exception = ex;
                }
                finally
                {
                    _isRunningTask = false;
                }
            },
            CancellationToken.None,
            TaskCreationOptions.None,
            TaskScheduler.FromCurrentSynchronizationContext());
        }

        GUI.enabled = true;

        var responseValue = response.GetValue(_activeService);

        if (responseValue != null)
        {
            string responseText = "Response:";
            responseType.GetProperties().ToList().ForEach(x =>
            {
                responseText += "\n";
                var value = x.GetValue(responseValue);
                responseText += $"{x.Name} = {value}";
            });

            EditorGUILayout.HelpBox(responseText, MessageType.Info);
        }

        if(_exception != null)
        {
            EditorGUILayout.HelpBox(_exception.ToString(), MessageType.Error);
        }
    }

    void RenderServices()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true), GUILayout.Height(32));
        EditorGUILayout.LabelField("Fandom Api Services", FandomEditorStyles.ListViewHeader, GUILayout.ExpandWidth(true));
        EditorGUILayout.EndHorizontal();
        foreach (var service in _services)
        {
            if (RenderServiceButton(service.Name, FandomEditorStyles.ListViewItem, GUILayout.Height(24)))
            {
                _activeService = (IFandomService)Activator.CreateInstance(service);
            }
        }
    }

    bool RenderServiceButton(string text, GUIStyle style, params GUILayoutOption[] options)
    {
        var content = new GUIContent(text);
        var rect = GUILayoutUtility.GetRect(content, style, options);

        var iconHeight = 12;
        var iconRect = new Rect(rect.x + rect.width - iconHeight - 4, rect.y + (rect.height / 2) - (iconHeight / 2), iconHeight, iconHeight);

        bool clicked = GUI.Button(rect, content, style);
        GUI.DrawTexture(iconRect, EditorGUIUtility.FindTexture("d_forward@2x"));

        return clicked;
    }
}
