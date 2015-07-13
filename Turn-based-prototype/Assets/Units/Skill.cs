using UnityEngine;
using System.Collections;

public class Skill : MonoBehaviour {

    public string Name;
    public int Range;
    public Sprite Icon;

    public int BaseDamage;
    public DamageType DamageType;
    public Status[] Debuffs;

    public int BaseHeal;
    public Status[] Buffs;

    public int CoolDown;
    private int cooldownTimer;
    public bool CanBeUsed() { return (cooldownTimer == 0); }

    public void Apply(UnitDataBase unit)
    {
        
    }
}
