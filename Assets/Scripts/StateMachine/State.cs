using UnityEngine;
using System;

public abstract class State 
{
    public Enum ID{get;protected set;}
    private float _startTime;
    protected float _runTime{get=>Time.time - _startTime;}
    public virtual Enum Tick()=>null;
    public virtual Enum FixedTick()=>null;
    public virtual Enum LateTick()=>null;
    public virtual Enum ManualTick()=>null;
    public virtual void OnEnter(Enum fromState) 
    {
        _startTime = Time.time;
    }
    public virtual void OnExit(Enum toState){}
}