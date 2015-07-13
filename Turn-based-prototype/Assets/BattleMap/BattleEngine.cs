using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;

public class BattleEngine : MonoBehaviour {

    [SerializeField]
	private HexBoard grid;
    [SerializeField]
    private UnitData activeUnit;
    public UnitData ActiveUnit
    {
        get { return activeUnit;}
        set
        {
            markReachableHexes(ActiveUnit,value);
            activeUnit = value;
        }
    }
    [SerializeField]
    private int attDefBalanceConstant = 5;

    private Vector2 positionToClean;
    private bool inWaitingTurn = false;

    public GameObject[] InitialUnits;
    public Queue<UnitData> ThisTurnQueue = new Queue<UnitData>();
    public Queue<UnitData> WaitingQueue = new Queue<UnitData>();
    public Queue<UnitData> NextTurnQueue = new Queue<UnitData>();


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
    public void AttackUnit(UnitData unit)
    {
        if (dealDamage(ActiveUnit, unit))
        {
            positionToClean = ActiveUnit.Position;
            GoToNextUnit(NextTurnQueue);
        }
    }
    public void MoveUnit(Vector2 to)
    {
        if (MoveUnit(ActiveUnit.GetComponent<UnitData>(), to))
        {
            string msg = string.Format("{0} moved to {1},{2}.", ActiveUnit.Name, ActiveUnit.Position.x, ActiveUnit.Position.y);
            Debug.Log(msg);
            GoToNextUnit(NextTurnQueue);
        }
    }

    //Dealing Damage
    private bool dealDamage(UnitData attacker, UnitData defender)
    {
        if (!isInRange(attacker,defender.Position,attacker.AttackRange) || attacker.Player == defender.Player)
            return false;

        float multiplier = (float)(attacker.Attack + attDefBalanceConstant) / (float)(defender.Deffence + attDefBalanceConstant);
        float totalDamage = 0;

        //TODO Find a way to do unitform distribution through dmgMin dmgMax!!
        for(int i=0; i < attacker.NumberOfUnits; i++)
        {
            float roll = UnityEngine.Random.Range(attacker.MinDamage, attacker.MaxDamage);
            totalDamage += (roll);
        }
        totalDamage *= (multiplier);
        int damage = (int)totalDamage;

        int defUnits = defender.NumberOfUnits;
        defender.Health -= damage;        
        int killedUnits = defUnits - defender.NumberOfUnits;

        if(defender.Health < 0)
            removeUnit(defender);
        
        string msg = string.Format("{0} attacked {1} and dealt {2} damage. ({3} units killed).", 
            attacker.Name, defender.Name, damage, killedUnits);
        Debug.Log(msg);
        return true;
    }


    //MOVEMENT
    private void GoToNextUnit(Queue<UnitData> queueToJoin)
    {
        if(ActiveUnit != null)
            queueToJoin.Enqueue(ActiveUnit);

        ThisTurnQueue = new Queue<UnitData>(ThisTurnQueue.OrderByDescending(unit => unit.Initiative));
                        
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
    private bool MoveUnit(UnitData unit, Vector2 to)
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


    private void markReachableHexes(UnitData previousUnit, UnitData nextUnit)
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

    private bool isInRange(UnitData unit, Vector2 destination, int range)
    {
        return (hexDistance(unit.Position, destination) <= range);
    }
    private void removeUnit(UnitData unit)
    {
        List<UnitData> unitsToRemove = new List<UnitData> { unit };
        ThisTurnQueue = new Queue<UnitData>(ThisTurnQueue.Except(unitsToRemove));
        WaitingQueue = new Queue<UnitData>(WaitingQueue.Except(unitsToRemove));
        NextTurnQueue = new Queue<UnitData>(NextTurnQueue.Except(unitsToRemove));
        Destroy(unit.gameObject);
    }
    private bool IsLandable(Vector2 place)
        { return !(grid[place]).HasObstacle; }

}
