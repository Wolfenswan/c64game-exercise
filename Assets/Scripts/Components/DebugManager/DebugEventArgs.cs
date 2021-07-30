using System;

public class DebugEventArgs : EventArgs
{
    public DebugEventArgs(string debugText, DebugLogType logType, DebugLogLevel logLevel, bool overrideLogLevelRestrictions = false)
    {
        DebugText = debugText;
        LogLevel = logLevel;
        LogType = logType;
        OverrideLogLevelRestrictions = overrideLogLevelRestrictions;
    }

    public string DebugText{get; private set;}
    public DebugLogLevel LogLevel{get; private set;}
    public DebugLogType LogType{get; private set;}
    public bool OverrideLogLevelRestrictions{get; private set;} = false;
}