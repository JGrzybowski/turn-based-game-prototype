using UnityEngine;
using System.Collections;

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

    public int Attack;
    public int AttackRange;
    public int Deffence;

    public int MinDamage;
    public int MaxDamage;

    public int NumberOfUnits
    {
        get { return Health / BaseHealth + ((Health % BaseHealth > 0) ? 1 : 0); }
    }

    public void DealDamage()
    {
        GetComponentInParent<BattleEngine>().AttackUnit(this);
    }
}
