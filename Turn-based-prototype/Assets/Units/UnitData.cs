using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public enum Stat { Attack, Defence, AttackRange, MinDamage, MaxDamage }

public class UnitData : MonoBehaviour {
    public string Name;
    public PlayerColor Player;
    public Vector2 Position;

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
        get { return attack + CalculateStatusChange(Stat.Attack); }
        set { attack = value; }
    }

    [SerializeField]
    private int defence;
    public int Defence
    {
        get { return defence + CalculateStatusChange(Stat.Defence); }
        set { defence = value; }
    }

    [SerializeField]
    private int attackRange;
    public int AttackRange
    {
        get { return attackRange + CalculateStatusChange(Stat.AttackRange); }
        set { attackRange = value; }
    }

    [SerializeField]
    private int minDamage;
    public int MinDamage
    {
        get { return minDamage + CalculateStatusChange(Stat.MinDamage); }
        set { minDamage = value; }
    }

    [SerializeField]
    private int maxDamage;
    public int MaxDamage
    {
        get { return maxDamage + CalculateStatusChange(Stat.MaxDamage); }
        set { maxDamage = value; }
    }

    public Skill[] Skills;
    public List<Status> Statuses = new List<Status>();

    public int NumberOfUnits
    {
        get { return Health / BaseHealth + ((Health % BaseHealth > 0) ? 1 : 0); }
    }

    public void DealDamage()
    {
        GetComponentInParent<BattleEngine>().AttackUnit(this);
    }
    public int CalculateStatusChange(Stat stat)
    {
        return Statuses.Where(Status => Status.Rule.Stat == stat).Sum(status => status.Rule.Change);
    }

}
