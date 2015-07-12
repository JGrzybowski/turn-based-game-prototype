using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;

public class BattleEngine : MonoBehaviour {

    [SerializeField]
	private HexBoard grid;
    [SerializeField]
    private UnitData activeUnit;
    [SerializeField]
    private int attDefBalanceConstant = 5;

    public UnitData ActiveUnit
    {
        get { return activeUnit;}
        set
        {
            //change active Unit
            activeUnit = value;
            //Mark Proper cells
            markReachableHexes(activeUnit);
        }
    }

    public GameObject[] InitialUnits;
    public Queue<UnitData> ThisTurnQueue = new Queue<UnitData>();
    public Queue<UnitData> WaitingQueue = new Queue<UnitData>();
    public Queue<UnitData> NextTurnQueue = new Queue<UnitData>();

    public bool IsLandable(Vector2 place)
        { return !(grid[place]).HasObstacle; }

    private void Start()
    {
        for (int i= 0; i< InitialUnits.Count(); i++)
        {
            ThisTurnQueue.Enqueue(SpawnExampleUnit(InitialUnits[i]));
        }
        MoveToNextUnit();
    }

    //Dealing Damage
    public void AttackUnit(UnitData unit)
    {
        if (dealDamage(ActiveUnit, unit))
            MoveToNextUnit();
    }

    private bool dealDamage(UnitData attacker, UnitData defender)
    {
        if (!IsInRange(attacker,defender.Position,attacker.AttackRange) || attacker.Player == defender.Player)
            return false;

        float roll = UnityEngine.Random.Range(attacker.MinDamage, attacker.MaxDamage);
        float multiplier = attacker.Attack + attDefBalanceConstant / (defender.Deffence + attDefBalanceConstant);
        int damage = (int)(roll * multiplier);
        
        defender.numberOfUnits -= damage / defender.MaxHealth;
        defender.Health -= damage % defender.MaxHealth;
        if(defender.Health < 0)
        {
            defender.numberOfUnits -= 1;
            defender.Health += defender.MaxHealth;
        }
        if(defender.numberOfUnits < 1)
        {
            List<UnitData> unitsToRemove = new List<UnitData>{ defender };
            ThisTurnQueue = new Queue<UnitData>(ThisTurnQueue.Except(unitsToRemove));
            WaitingQueue = new Queue<UnitData>(WaitingQueue.Except(unitsToRemove));
            NextTurnQueue = new Queue<UnitData>(NextTurnQueue.Except(unitsToRemove));
        }
        return true;
    }

    //MOVEMENT
    public void MoveUnit(Vector2 to)
    {
        if (MoveUnit(ActiveUnit.GetComponent<UnitData>(), to))
            MoveToNextUnit();
    }

    private void MoveToNextUnit()
    {
        if(ActiveUnit != null)
            NextTurnQueue.Enqueue(ActiveUnit);

        ThisTurnQueue = new Queue<UnitData>(ThisTurnQueue.OrderByDescending(unit => unit.Initiative));
                        
        if (ThisTurnQueue.Count > 0)
        {
            ActiveUnit = ThisTurnQueue.Dequeue();
        }
        else if(WaitingQueue.Count > 0)
        {
            ActiveUnit = WaitingQueue.Dequeue();
        }
        else
        {
            var tmpQueue = ThisTurnQueue;
            ThisTurnQueue = NextTurnQueue;
            NextTurnQueue = tmpQueue;
            ActiveUnit = null;
            MoveToNextUnit();
        }        
    }

    private bool MoveUnit(UnitData unit, Vector2 to)
    {
        if (!IsLandable(to))
            return false;
        if (!IsInRange(unit, to, unit.Speed))
            return false;
        unit.Position = to;
        setUnitPosition(unit, to);
        markReachableHexes(unit);
        return true;
    }
    private void setUnitPosition(UnitData unit, Vector2 position)
    {
        unit.transform.SetParent((grid[position]).transform);
        unit.GetComponent<RectTransform>().position = unit.GetComponentInParent<Hex>().transform.position;
    }


    public UnitData SpawnExampleUnit(GameObject prefab)
    {
        var obj = (GameObject)Instantiate(prefab, Vector3.zero, Quaternion.identity);
        var unit = obj.GetComponent<UnitData>();
        setUnitPosition(unit, unit.Position);
        obj.GetComponent<RectTransform>().localScale = Vector3.one;
        return unit;
    }

    private void markReachableHexes(UnitData unit)
    {
        if (unit == null) return;
        Hex hex = grid[unit.Position];

        foreach(var h in grid)
        {
            h.IsInMoveRange = IsInRange(unit, h.Position, unit.Speed);
        }
    }

    private int hexDistance(int qa, int ra, int qb,int rb)
        { return (Math.Abs(qa - qb) + Math.Abs(ra - rb) + Math.Abs(qa + ra - qb - rb)) / 2; }
    private int hexDistance(Vector2 a, Vector2 b)
        { return hexDistance((int)a.x, (int)a.y, (int)b.x, (int)b.y); }
    private bool IsInRange(UnitData unit, Vector2 destination, int range)
    {
        return (hexDistance(unit.Position, destination) <= range);
    }

}
