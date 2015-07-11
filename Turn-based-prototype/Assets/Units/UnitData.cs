using UnityEngine;
using System.Collections;

public class UnitData : MonoBehaviour {
    public PlayerColor Player;
    public Vector2 Position;

    public int Speed;
    public int Initiative;

    public int Health;
    public int MaxHealth;
    public int Mana;
    public int MaxMana;

    public int Attack;
    public int AttackRange;
    public int Deffence;

    public int MinDamage;
    public int MaxDamage;

    public int numberOfUnits = 1;

    public void DealDamage()
    {
        //Debug.Log("Click at" + Position);
        GetComponentInParent<BattleEngine>().AttackUnit(this);
    }
}
