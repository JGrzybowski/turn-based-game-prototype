using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum StatusType { Buff, Debuff }

public class Status : ScriptableObject
{
    public int Duration;
    public bool Parmanent;
    public bool Removable;
    public StatusType Type;
    public StatusRule Rule;
}