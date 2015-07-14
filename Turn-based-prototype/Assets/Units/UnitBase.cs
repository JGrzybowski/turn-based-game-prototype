using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public enum Stat { Attack, Defence, AttackRange, MinDamage, MaxDamage }

public class UnitBase : MonoBehaviour {
    public string Name;
    public PlayerColor Player;
    public Vector2 Position;
   
    #region Stats
    public int Speed;
    public int Initiative;

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
        this.Health -= dmg;
        int killedUnits = defUnits - this.NumberOfUnits;

        if (this.Health < 0)
        {
            GetComponentInParent<BattleEngine>().RemoveUnit(this);
        }

        string msg = string.Format("{0} attacks {1} and deal {2} damage. ({3} {1} killed).",
          enemy.Name, this.Name, dmg, killedUnits);
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
        totalDamage *= (multiplier);
        return (int)totalDamage;
    }
    public void AttackMeele(UnitBase enemy) { OnAttackMeele(enemy, AttackType.Meele); }
    public void AttackRanged(UnitBase enemy) { OnAttackRanged(enemy, AttackType.Ranged); }
}

public enum AttackType
{
    Meele, Ranged, Counterattack, Spell
}