using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;

public class BattleEngine : MonoBehaviour {

    [SerializeField]
	private HexBoard grid;
    [SerializeField]
    private UnitDataBase activeUnit;
    public UnitDataBase ActiveUnit
    {
        get { return activeUnit;}
        set
        {
            markReachableHexes(ActiveUnit,value);
            activeUnit = value;
        }
    }
    
    public int attDefBalanceConstant = 5;

    private Vector2 positionToClean;
    private bool inWaitingTurn = false;

    public GameObject[] InitialUnits;
    public Queue<UnitDataBase> ThisTurnQueue = new Queue<UnitDataBase>();
    public Queue<UnitDataBase> WaitingQueue = new Queue<UnitDataBase>();
    public Queue<UnitDataBase> NextTurnQueue = new Queue<UnitDataBase>();


    private void Start()
    {
        for (int i= 0; i< InitialUnits.Count(); i++)
        {
            ThisTurnQueue.Enqueue(SpawnExampleUnit(InitialUnits[i]));
        }
        GoToNextUnit(null);
    }

    public void WaitUnit()
    {
        if (!inWaitingTurn)
        {
            positionToClean = ActiveUnit.Position;
            GoToNextUnit(WaitingQueue);
        }
    }
    public void AttackUnit(UnitDataBase unit)
    {
        if (dealDamage(ActiveUnit, unit))
        {
            positionToClean = ActiveUnit.Position;
            GoToNextUnit(NextTurnQueue);
        }
    }
    public void MoveUnit(Vector2 to)
    {
        if (MoveUnit(ActiveUnit.GetComponent<UnitDataBase>(), to))
        {
            string msg = string.Format("{0} moved to {1},{2}.", ActiveUnit.Name, ActiveUnit.Position.x, ActiveUnit.Position.y);
            Debug.Log(msg);
            GoToNextUnit(NextTurnQueue);
        }
    }

    //Dealing Damage
    private bool dealDamage(UnitDataBase attacker, UnitDataBase defender)
    {
        if (!isInRange(attacker,defender.Position,attacker.AttackRange) || attacker.Player == defender.Player)
            return false;

        int distance = hexDistance(attacker.Position, defender.Position);
        if (distance > 1)
            attacker.AttackRanged(defender);
        else
            attacker.AttackMeele(defender);

        return true;
    }


    //MOVEMENT
    private void GoToNextUnit(Queue<UnitDataBase> queueToJoin)
    {
        if(ActiveUnit != null)
            queueToJoin.Enqueue(ActiveUnit);

        ThisTurnQueue = new Queue<UnitDataBase>(ThisTurnQueue.OrderByDescending(unit => unit.Initiative));
                        
        if (ThisTurnQueue.Count > 0)
        {
            ActiveUnit = ThisTurnQueue.Dequeue();
        }
        else if(WaitingQueue.Count > 0)
        {
            ActiveUnit = WaitingQueue.Dequeue();
            this.inWaitingTurn = true;
        }
        else
        {
            var tmpQueue = ThisTurnQueue;
            ThisTurnQueue = NextTurnQueue;
            NextTurnQueue = tmpQueue;
            this.inWaitingTurn = false;
            ActiveUnit = null;
            GoToNextUnit(null);
        }        
    }
    private bool MoveUnit(UnitDataBase unit, Vector2 to)
    {
        if (!IsLandable(to))
            return false;
        if (!isInRange(unit, to, unit.Speed))
            return false;
        this.positionToClean = unit.Position;
        unit.Position = to;
        setUnitPosition(unit, to);
        //markReachableHexes(unit);
        return true;
    }
    private void setUnitPosition(UnitDataBase unit, Vector2 position)
    {
        unit.transform.SetParent((grid[position]).transform);
        unit.GetComponent<RectTransform>().position = unit.GetComponentInParent<Hex>().transform.position;
    }
    
    public UnitDataBase SpawnExampleUnit(GameObject prefab)
    {
        var obj = (GameObject)Instantiate(prefab, Vector3.zero, Quaternion.identity);
        var unit = obj.GetComponent<UnitDataBase>();
        setUnitPosition(unit, unit.Position);
        obj.GetComponent<RectTransform>().localScale = Vector3.one;
        return unit;
    }


    private void markReachableHexes(UnitDataBase previousUnit, UnitDataBase nextUnit)
    {
        //TODO use more efficient algorithm:
        //  - deselect cells from previous unit 
        //  - mark those for the new one
        if (this.positionToClean != null && previousUnit != null)
        {
            foreach (var position in grid.hexesInRange(this.positionToClean, previousUnit.Speed))
            {
                grid[position].IsInMoveRange = false;
            }
        }
        if (nextUnit != null)
        {
            foreach (var position in grid.hexesInRange(nextUnit.Position, nextUnit.Speed))
            {
                grid[position].IsInMoveRange = true;
            }
        }
    }
    private int hexDistance(int qa, int ra, int qb,int rb)
        { return (Math.Abs(qa - qb) + Math.Abs(ra - rb) + Math.Abs(qa + ra - qb - rb)) / 2; }
    private int hexDistance(Vector2 a, Vector2 b)
        { return hexDistance((int)a.x, (int)a.y, (int)b.x, (int)b.y); }

    private bool isInRange(UnitDataBase unit, Vector2 destination, int range)
    {
        return (hexDistance(unit.Position, destination) <= range);
    }
    public void RemoveUnit(UnitDataBase unit)
    {
        List<UnitDataBase> unitsToRemove = new List<UnitDataBase> { unit };
        ThisTurnQueue = new Queue<UnitDataBase>(ThisTurnQueue.Except(unitsToRemove));
        WaitingQueue = new Queue<UnitDataBase>(WaitingQueue.Except(unitsToRemove));
        NextTurnQueue = new Queue<UnitDataBase>(NextTurnQueue.Except(unitsToRemove));
        Destroy(unit.gameObject);
    }
    private bool IsLandable(Vector2 place)
        { return !(grid[place]).HasObstacle; }

}
