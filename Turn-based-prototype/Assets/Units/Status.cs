using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum StatusType { Buff, Debuff }

public class StatusRule
{
    public Stat Stat;
    public int Change;
}

public class Status
{
    public int Duration;
    public bool Parmanent;
    public bool Removable;
    public StatusType Type;
    public StatusRule Rule;
}