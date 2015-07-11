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
            //change active Unit
            activeUnit = value;
            //Mark Proper cells
            markReachableHexes(activeUnit);
        }
    }

    public GameObject[] InitialUnits;

    public List<UnitData> TurnQueue;

    public bool IsLandable(Vector2 place)
    { return !(grid[place]).HasObstacle; }

    private void Start()
    {
        for (int i= 0; i< InitialUnits.Count(); i++)
        {
            TurnQueue.Add(SpawnExampleUnit(InitialUnits[i]));
        }
        BattleLoop();
    }

    private void BattleLoop()
    {
        TurnQueue.OrderBy(unit => unit.Initiative);
        ActiveUnit = TurnQueue.First();        
    }

    //MOVEMENT
    public void MoveUnit(Vector2 to)
    {
        if (MoveUnit(ActiveUnit.GetComponent<UnitData>(), to))
        {
            TurnQueue.Add(TurnQueue.First());
            TurnQueue.RemoveAt(0);
            ActiveUnit = TurnQueue.First();
        }
    }
    private bool MoveUnit(UnitData unit, Vector2 to)
    {
        if (!IsLandable(to))
            return false;
        if (!IsInMoveRange(unit, to))
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
        //obj.transform.SetParent(grid.transform);
        obj.GetComponent<RectTransform>().localScale = Vector3.one;
        var unit = obj.GetComponent<UnitData>();
        setUnitPosition(unit, unit.Position);
        return unit;
    }

    private void markReachableHexes(UnitData unit)
    {
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
