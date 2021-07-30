using System;

public interface IDebugMonitor
{   
    event EventHandler<DebugEventArgs> DebugEvent;
    event Action<object, string> DebugLogEvent; // Treated as (string, DebugLogType.CONSOLE, DebugLogLevel.NORMAL, overrideLogLevelStrictions = false)
    event Action<object, string> DebugWarningEvent; // Treated as (string, DebugLogType.CONSOLE, DebugLogLevel.WARNING, overrideLogLevelStrictions = false) 
    event Action<object, string> DebugErrorEvent; // Treated as (string, DebugLogType.CONSOLE, DebugLogLevel.ERROR, overrideLogLevelStrictions = false)
    event Action<object, string> DebugTextUpdateEvent; // Treated as (string, DebugLogType.TEXT, DebugLogLevel.NORMAL, overrideLogLevelStrictions = true)

    //void DebugRegister(); // Should be called during Start()
    //void DebugDeregister(); // Should be called when object is disabled or destroyed
    void DebugRaiseEvent(string debugText, DebugLogType logType = DebugLogType.CONSOLE, DebugLogLevel logLevel = DebugLogLevel.NORMAL, bool overrideLogLevelRestrictions = false);

    /*
    DEFAULT IMPLEMENTATION EXAMPLES:
    
    public void DebugInitialize()
    {   
        var debugTitleText = $"{this}";
        var textField = DebugManager.DebugCreateTextField(this.gameObject, Vector3.zero); // Optional, can be null
        DebugManager.Instance.InitializeDebugMonitor<MyCustomClass>(this, debugTitleText, _dbgMaxLogLevel, textField);
    }

    public void DebugDeregister()
    {
        DebugManager.Instance.DeregisterObject(this);
    }

    public void DebugRaiseEvent(string debugText, DebugLogLevel logLevel = DebugLogType.CONSOLE, DebugLogType logType = DebugLogLevel.NORMAL, bool overrideLogLevelRestrictions = false)
    {
        var args = new DebugEventArgs(debugText, logLevel, logType);
        DebugEvent?.Invoke(this, args);
    }

    EXAMPLE EVENT USAGE:

    DebugRaiseEvent("This should not happen", DebugLogType.CONSOLE, DebugLogLevel.ERROR);
    DebugRaiseEvent($"SomeInteger has the value: {someInteger}", DebugLogType.CONSOLE, DebugLogLevel.NORMAL);
    DebugRaiseEvent($"CurrentState: {myState}\nAlertness: {alertLevel}",  DebugLogType.TEXT, DebugLogLevel.NORMAL, overrideLogLevelRestrictions = true);

    */
}