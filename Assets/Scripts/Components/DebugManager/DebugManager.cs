using UnityEngine;
using System.Collections.Generic;
using NEINGames.Singleton;
using TMPro;

public class DebugManager : Singleton<DebugManager>
{
    [SerializeField, Tooltip("If false any debug output is disabled, including those events overriding the log-level filter.")] bool _debugEnabled = true;
    [SerializeField, Tooltip("Specifies the highest logging level to display. Debug output will still show if overriden by individual events.")] DebugLogLevel _debugMaxLogLevel = DebugLogLevel.NORMAL;
    [SerializeField, Tooltip("Specifies the template to base monitoring fields on. Can be null.")] GameObject _textFieldTemplate = null;

    Dictionary<object, DebugMonitoredObject> _monitoredObjects = new Dictionary<object, DebugMonitoredObject>();

    void Awake() 
    {
        InitializeSingleton(this);
    }

    public void RegisterObject<T>(T monitoredInstance, string titleText, DebugLogLevel maxLogLevel, TextMeshPro textField = null) where T : IDebugMonitor
    {   
        if(!_debugEnabled) return;

        if (_monitoredObjects.ContainsKey(monitoredInstance))
        {
            Debug.LogError($"{this} | Tried registering already registered object: {monitoredInstance}");
            return;
        }
        
        _monitoredObjects[monitoredInstance] = new DebugMonitoredObject(titleText, maxLogLevel, textField);
        monitoredInstance.DebugEvent += OnDebugEvent;
        monitoredInstance.DebugLogEvent += OnDebugLogEvent;
        monitoredInstance.DebugWarningEvent += OnDebugWarningEvent;
        monitoredInstance.DebugErrorEvent += OnDebugErrorEvent;
        monitoredInstance.DebugTextUpdateEvent += OnDebugTextUpdateEvent;
    }

    public void DeregisterObject(object monitoredObject)
    {   
        var obj = (IDebugMonitor) monitoredObject;
        obj.DebugEvent -= OnDebugEvent;
        obj.DebugLogEvent -= OnDebugLogEvent;
        obj.DebugWarningEvent -= OnDebugWarningEvent;
        obj.DebugErrorEvent -= OnDebugErrorEvent;
        obj.DebugTextUpdateEvent -= OnDebugTextUpdateEvent;
        Destroy(_monitoredObjects[monitoredObject].TextField.gameObject);
        _monitoredObjects.Remove(monitoredObject);
    }

    #region shorthands
    void OnDebugLogEvent(object sender, string dbgText) => OnDebugEvent(sender, new DebugEventArgs(dbgText, DebugLogType.CONSOLE, DebugLogLevel.NORMAL));
    void OnDebugWarningEvent(object sender, string dbgText) => OnDebugEvent(sender, new DebugEventArgs(dbgText, DebugLogType.CONSOLE, DebugLogLevel.WARNING));
    void OnDebugErrorEvent(object sender, string dbgText) => OnDebugEvent(sender, new DebugEventArgs(dbgText, DebugLogType.CONSOLE, DebugLogLevel.ERROR));
    void OnDebugTextUpdateEvent(object sender, string dbgText) => OnDebugEvent(sender, new DebugEventArgs(dbgText, DebugLogType.TEXT, DebugLogLevel.NORMAL));
    #endregion

    void OnDebugEvent(object sender, DebugEventArgs debugEventArgs)
    {   
        var dbgText = debugEventArgs.DebugText;
        var logLevel = debugEventArgs.LogLevel;
        var logType = debugEventArgs.LogType;

        var logToConsole = logType == DebugLogType.CONSOLE || logType == DebugLogType.CONSOLE_AND_TEXT;
        var logToText = logType == DebugLogType.TEXT || logType == DebugLogType.CONSOLE_AND_TEXT;
        
        if (_monitoredObjects.TryGetValue(sender, out DebugMonitoredObject debugObjectData))
        {   
            if (!debugEventArgs.OverrideLogLevelRestrictions && (logLevel > debugObjectData.MaxLogLevel || logLevel > _debugMaxLogLevel))
                return;

            if (logToText && !debugObjectData.HasTextField)
            {
                Debug.LogError($"{this} | Trying to update text field but no text field is assigned to {sender}.");
                return;
            }

            var titleText = debugObjectData.TitleText;
            switch (logType)
            {
                case DebugLogType.CONSOLE:
                    LogToConsole(dbgText, titleText, logLevel);
                    break;
                case DebugLogType.TEXT:
                    UpdateTextMonitor(dbgText, titleText, debugObjectData.TextField, logLevel);
                    break;
                case DebugLogType.CONSOLE_AND_TEXT:
                    UpdateTextMonitor(dbgText, titleText, debugObjectData.TextField, logLevel);
                    LogToConsole(dbgText, titleText, logLevel);
                    break;
                default:
                    Debug.LogWarning($"{this} | Unknown LogType: {logType}.");
                    break;
            }
        } else 
        {
            Debug.LogError($"{this} | {sender} not found in monitored objects. Has it been initialized?");
        }
    }

    void LogToConsole(string dbgText, string titleText, DebugLogLevel logLevel)
    {   
        var output = $"{titleText} | {dbgText}";

        switch (logLevel)
        {
            case DebugLogLevel.NORMAL:
                Debug.Log(output);
                break;
            case DebugLogLevel.WARNING:
                Debug.LogWarning(output);
                break;
            case DebugLogLevel.ERROR:
                Debug.LogError(output);
                break;
            default:
                Debug.LogWarning($"{this} | Unknown LogLevel: {logLevel}.");
                break;
        }
    }

    void UpdateTextMonitor(string dbgText, string titleText, TextMeshPro textField, DebugLogLevel logLevel)
    {
        Color color;
        switch (logLevel)
        {
            case DebugLogLevel.NORMAL:
                color = Color.white;
                break;
            case DebugLogLevel.WARNING:
                color = Color.yellow;
                break;
            case DebugLogLevel.ERROR:
                color = Color.red;
                break;
            default:
                Debug.LogWarning($"{this} | Unknown LogLevel: {logLevel}.");
                break;

        }
        textField.text = $"{titleText}\n{dbgText}";
    }

    // TODO deliberate if this needs to be static
    public static TextMeshPro CreateTextFieldContainer(GameObject parent, Vector3 localPosition)
    {   
        var template = DebugManager.Instance._textFieldTemplate;

        if (template == null)
        {
            Debug.LogError($"GameManager.CreateTextFieldContainer | Trying to create text-field but no template has been defined.");
            return null;
        }

        var newObj = Instantiate(template, parent.transform);
        newObj.transform.localPosition = localPosition;
        return newObj.GetComponent<TextMeshPro >();
    }
}