using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;

public enum Stat { Attack, Defence, AttackRange, MinDamage, MaxDamage, Initiative,
    PhysicalResistance,
    RangedResistance,
    FireResistance,
    IceResistance,
    MagicResistance
}

public class UnitBase : MonoBehaviour {
    public string Name;
    public PlayerColor Player;
    public Vector2 Position;
       
    #region Stats
    public int Speed;
    [SerializeField]
    private int initiative;
    public int Initiative
    {
        get { return initiative + calculateStatChange(Stat.Initiative); }
        set { initiative = value; }
    }

    public int Health;
    public int BaseHealth;
    public int Mana;
    public int MaxMana;
    
    [SerializeField]
    private int attack;
    public int Attack
    {
        get { return attack + calculateStatChange(Stat.Attack); }
        set { attack = value; }
    }

    [SerializeField]
    private int defence;
    public int Defence
    {
        get { return defence + calculateStatChange(Stat.Defence); }
        set { defence = value; }
    }

    [SerializeField]
    private int attackRange;
    public int AttackRange
    {
        get { return attackRange + calculateStatChange(Stat.AttackRange); }
        set { attackRange = value; }
    }

    [SerializeField]
    private int minDamage;
    public int MinDamage
    {
        get { return minDamage + calculateStatChange(Stat.MinDamage); }
        set { minDamage = value; }
    }

    [SerializeField]
    private int maxDamage;
    public int MaxDamage
    {
        get { return maxDamage + calculateStatChange(Stat.MaxDamage); }
        set { maxDamage = value; }
    }

    [SerializeField]
    private int physicalResistance;
    public int PhysicalResistance
    {
        get { return physicalResistance + calculateStatChange(Stat.PhysicalResistance); }
        set { physicalResistance = value; }
    }

    [SerializeField]
    private int rangedResistance;
    public int RangedResistance
    {
        get { return rangedResistance + calculateStatChange(Stat.RangedResistance); }
        set { rangedResistance = value; }
    }

    [SerializeField]
    private int fireResistance;
    public int FireResistance
    {
        get { return fireResistance + calculateStatChange(Stat.FireResistance); }
        set { fireResistance = value; }
    }

    [SerializeField]
    private int iceResistance;
    public int IceResistance
    {
        get { return iceResistance + calculateStatChange(Stat.IceResistance); }
        set { iceResistance = value; }
    }

    [SerializeField]
    private int magicResistance;
    public int MagicResistance
    {
        get { return magicResistance + calculateStatChange(Stat.MagicResistance); }
        set { magicResistance = value; }
    }

    public SpellBase[] Spells;
    public List<Status> Statuses = new List<Status>();
    public int NumberOfUnits
    {
        get { return Health / BaseHealth + ((Health % BaseHealth > 0) ? 1 : 0); }
    }

    //Possible usage of flags?
    public DamageType DamageType; 
    #endregion

    private void Start()
    {
        this.OnDamageMeele += CounterAttack;
        this.OnAttackMeele += DealDamage;
        this.OnAttackRanged += DealDamage;
    }

    //Recieves damage
    public int Damage(UnitBase enemy, int dmg, DamageType dmgType, AttackType attType)
    {
        int defUnits = this.NumberOfUnits;
        float resistance = getResistanceForDamageType(dmgType) / 100f;
        int finalDamage = (int)(dmg * (1 - resistance));
        this.Health -= finalDamage;
        int killedUnits = defUnits - this.NumberOfUnits;

        if (this.Health < 0)
        {
            GetComponentInParent<BattleEngine>().RemoveUnit(this);
        }

        string msg = string.Format("{0} attacks {1} and deal {2} damage. ({3} {1} killed).",
                                    enemy.Name, this.Name, finalDamage, killedUnits);
        Debug.Log(msg);

        switch (attType)
        {
            case AttackType.Meele:
                if(OnDamageMeele != null)
                    OnDamageMeele(enemy, attType);
                break;
            case AttackType.Ranged:
                if (OnDamageRanged != null)
                    OnDamageRanged(enemy, attType);
                break;
        }

        return killedUnits;
    }
    public void Heal(UnitBase healer, int value)
    {
        int maxThatCanBeHealed = this.NumberOfUnits * BaseHealth - this.Health;
        if (value > maxThatCanBeHealed)
            value = maxThatCanBeHealed;
        
        this.Health += value;
        string msg = string.Format("{0} heals {1} and from {2} damage.",
                                   healer.Name, this.Name, value);
        Debug.Log(msg);

    }
    //Deals damage to the enemy
    public void DealDamage(UnitBase enemy, AttackType attType)
    {
        int dmg = calculateDamage(this, enemy, this.DamageType);
        enemy.Damage(this, dmg, this.DamageType, attType);                
    }

    private void CounterAttack(UnitBase enemy, AttackType attType)
    {
        DealDamage(enemy, AttackType.Counterattack);
    }

    public void Click()
    {
        GetComponentInParent<BattleEngine>().UnitClicked(this);
    }

    private int calculateStatChange(Stat stat)
    {
        return Statuses.Where(Status => Status.Rule.Stat == stat).Sum(status => status.Rule.Change);
    }
    
    public delegate void AttackEventHandler(UnitBase unit, AttackType type);
    public delegate void DamageEventHandler(UnitBase unit, AttackType type);

    public event AttackEventHandler OnAttackMeele;
    public event AttackEventHandler OnAttackRanged;
    public event DamageEventHandler OnDamageMeele;
    public event DamageEventHandler OnDamageRanged;

    public int calculateDamage(UnitBase attacker, UnitBase defender, DamageType dmgType)
    {
        float multiplier = (float)(attacker.Attack + GetComponentInParent<BattleEngine>().attDefBalanceConstant) / (float)(defender.Defence + GetComponentInParent<BattleEngine>().attDefBalanceConstant);
        float totalDamage = 0;
        
        //TODO Find a way to do unitform distribution through dmgMin dmgMax!!
        for (int i = 0; i < attacker.NumberOfUnits; i++)
        {
            float roll = UnityEngine.Random.Range(attacker.MinDamage, attacker.MaxDamage);
            totalDamage += (roll);
        }
        totalDamage *= multiplier;
        return (int)totalDamage;
    }

    private float getResistanceForDamageType(DamageType dmgType)
    {
        switch (dmgType)
        {
            case DamageType.Physical:
                return PhysicalResistance;
            case DamageType.PhysicalRanged:
                return RangedResistance;
            case DamageType.Fire:
                return FireResistance;
            case DamageType.Ice:
                return IceResistance;
            case DamageType.Magic:
                return MagicResistance;
            default:
                throw new NotImplementedException("You've forgot to implement add DamageType <-> resistance stat mapping for " + dmgType.ToString() +".");
        }
    }

    public void AttackMeele(UnitBase enemy) { OnAttackMeele(enemy, AttackType.Meele); }
    public void AttackRanged(UnitBase enemy) { OnAttackRanged(enemy, AttackType.Ranged); }
    public void UpdateAfterTurnsEnd() { UpdateStatuses(); UpdateCooldowns(); }
    private void UpdateCooldowns()
    {
        foreach (var spell in Spells.Where(s => s.CooldownTimer > 0))
        {
            spell.CooldownTimer--;
        }
    }
    private void UpdateStatuses()
    {
        foreach(var status in Statuses.Where(status => !status.Parmanent))
        {
            status.Duration--;
        }
        Statuses.RemoveAll(status => status.Duration <= 0);
    }
}

public enum AttackType
{
    Meele, Ranged, Counterattack, Spell
}