using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum SpellTarget { Enemy, Ally, Position}

public abstract class SpellBase : MonoBehaviour {

    public string Name;
    public abstract List<Vector2> Area { get; }
    public Sprite Icon;
    public SpellTarget Target;

    public int BaseDamage;
    public DamageType DamageType;
    public Status[] Debuffs;

    public int BaseHeal;
    public Status[] Buffs;

    public int CoolDown;
    private int cooldownTimer;
    public bool CanBeUsed() { return (cooldownTimer == 0); }

    public abstract void Apply(UnitBase caster, UnitBase unit);
 
}
