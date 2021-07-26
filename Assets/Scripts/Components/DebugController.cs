using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

//! Work-in-progress
// IDEA: dynamically create the TMP component
// IDEA: Add an event/broadcast systems so debug can logged independently from the update loop (i.e. from within updates)
// IDEA: Make a Manager-type singleton using event system (problem: would need to individually proscribe to each object? but could be solved some clever generic method writing)
// - use DebugMonitor Objects?
// IDEA: With the manager, create TMP-text objects that follow their objects (could be a toggle as well)

// Use Interfaces to standardize DebugEvents on monitored objects

public class DebugMonitor
{
    // Awake
    // Create TMP component
}

[RequireComponent (typeof(TextMeshPro))]
public class DebugController : MonoBehaviour 
{
    [SerializeField] bool _debugText;
    [SerializeField] bool _debugLog;

    public List<dynamic> _monitoredVariablesLog{get; private set;} = new List<dynamic>();
    public List<dynamic> _monitoredVariablesText{get; private set;} = new List<dynamic>();

    // 'void Awake() 
    // {
    //     _debug = transform.Find("DebugText").GetComponent<TextMeshPro>();
    // }

    public void MonitorVariableToLog(dynamic variable)
    {   
        if (_monitoredVariablesLog.Contains(variable))
            _monitoredVariablesLog.Remove(variable);
        else
            _monitoredVariablesLog.Add(variable);
    }

    public void MonitorVariableToText(dynamic variable)
    {   
        if (_monitoredVariablesText.Contains(variable))
            _monitoredVariablesText.Remove(variable);
        else
            _monitoredVariablesText.Add(variable);
    }

    // public UpdateText()
    // {
        
    // }'
    
}