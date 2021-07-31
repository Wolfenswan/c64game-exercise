using System;

// IMPLEMENTATION EXAMPLES:

/* REGISTRATION
Without text-field:
    DebugManager.Instance.RegisterObject<MyCustomClass>(this, $"{this}", DebugLogLevel.NORMAL);

With dynamically generated text-field, as a child of the monitored object:
    var textField = DebugManager.CreateTextFieldContainer(gameObject, new Vector3(0,1,0));
    DebugManager.Instance.RegisterObject<MyCustomClass>(this, $"{this}", DebugLogLevel.NORMAL, textField);
*/

/* EVENT RAISING

Using the generic event:
    public void DebugRaiseEvent(string debugText, DebugLogLevel logLevel = DebugLogType.CONSOLE, DebugLogType logType = DebugLogLevel.NORMAL, bool overrideLogLevelRestrictions = false)
    {
        var args = new DebugEventArgs(debugText, logLevel, logType);
        DebugEvent?.Invoke(this, args);
    }

    DebugRaiseEvent("This should not happen", DebugLogType.CONSOLE, DebugLogLevel.ERROR);
    DebugRaiseEvent($"SomeInteger has the value: {someInteger}", DebugLogType.CONSOLE, DebugLogLevel.NORMAL);
    DebugRaiseEvent($"A very important message that should always show as in-scene text.",  DebugLogType.TEXT, DebugLogLevel.NORMAL, overrideLogLevelRestrictions = true);

Using a short-hand:
    DebugLogEvent?.Invoke(this, "A short log statement");
*/

public interface IDebugMonitor
{   
    event EventHandler<DebugEventArgs> DebugEvent;
    event Action<object, string> DebugLogEvent; // Treated as (string, DebugLogType.CONSOLE, DebugLogLevel.NORMAL, overrideLogLevelStrictions = false)
    event Action<object, string> DebugWarningEvent; // Treated as (string, DebugLogType.CONSOLE, DebugLogLevel.WARNING, overrideLogLevelStrictions = false) 
    event Action<object, string> DebugErrorEvent; // Treated as (string, DebugLogType.CONSOLE, DebugLogLevel.ERROR, overrideLogLevelStrictions = false)
    event Action<object, string> DebugTextUpdateEvent; // Treated as (string, DebugLogType.TEXT, DebugLogLevel.NORMAL, overrideLogLevelStrictions = true)

    void DebugRaiseEvent(string debugText, DebugLogType logType = DebugLogType.CONSOLE, DebugLogLevel logLevel = DebugLogLevel.NORMAL, bool overrideLogLevelRestrictions = false);
}