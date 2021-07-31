using TMPro;

/*
DebugMonitoredObject is a custom container to store all required persistant information for debugging an object.
The global DebugManager stores these containers as values of a dictionary, assigning the monitored object as key.
*/
public class DebugMonitoredObject
{
    public DebugMonitoredObject(string titleText, DebugLogLevel maxLogLevel, TextMeshPro  textField)
    {
        TitleText = titleText;
        MaxLogLevel = maxLogLevel;
        TextField = textField;
    }

    public string TitleText{get; private set;}
    public TextMeshPro TextField{get; private set;}
    public DebugLogLevel MaxLogLevel{get; private set;}

    public bool HasTextField{get=>TextField != null;}
}