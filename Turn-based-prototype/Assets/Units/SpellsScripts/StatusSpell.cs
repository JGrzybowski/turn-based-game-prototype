using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class StatusSpell : CircularAreaSpell
{
    public Status status;
    public override void Apply(UnitBase caster, UnitBase unit)
    {
        var statusToAdd = ScriptableObject.CreateInstance<Status>();
        statusToAdd.Duration = status.Duration;
        statusToAdd.Parmanent = status.Parmanent;
        statusToAdd.Removable = status.Removable;
        statusToAdd.Rule = status.Rule;
        statusToAdd.Type = status.Type;
        unit.Statuses.Add(statusToAdd);
    }
}

