using UnityEngine;
using System.Collections;
using System;

public class BattleEngine : MonoBehaviour {

    [SerializeField]
	private HexBoard grid;
    public GameObject unit;
    //TODO Remove before sending!
    public GameObject ExampleUnit;

    public bool IsLandable(Vector2 place)
    { return !(grid[place]).HasObstacle; }

    private void Start()
    {
        SpawnExampleUnit();
    }
    //MOVEMENT
    public void MoveUnit(Vector2 from, Vector2 to)
    {
        UnitData unit = ((Hex)grid[from]).Unit;
        MoveUnit(unit, to);
    }
    public void MovePiece(Vector2 position)
    {
        MoveUnit(unit.GetComponent<UnitData>(), position);
    }
    public void MoveUnit(UnitData unit, Vector2 destination)
    {
        if (!IsLandable(destination))
            return;
        if (!IsInMoveRange(unit, destination))
            return;

        unit.Position = destination;
        unit.transform.SetParent((grid[destination]).transform);
        unit.GetComponent<RectTransform>().position = unit.GetComponentInParent<Hex>().transform.position;
        markReachableHexes(unit);
    }

    public void SpawnExampleUnit()
    {
        unit = (GameObject)Instantiate(ExampleUnit, Vector3.zero, Quaternion.identity);
        unit.transform.SetParent(grid.transform);
        MovePiece(new Vector2(0, 0));
        unit.GetComponent<RectTransform>().localScale = Vector3.one;
        MovePiece(new Vector2(0, 0));
    }

    private void markReachableHexes(UnitData unit)
    {
        Hex hex = grid[unit.Position];
        foreach(var h in grid)
        {
            h.IsInMoveRange = IsInMoveRange(unit, h.Position);
        }
    }

    private int hexDistance(int qa, int ra, int qb,int rb)
        { return (Math.Abs(qa - qb) + Math.Abs(ra - rb) + Math.Abs(qa + ra - qb - rb)) / 2; }
    private int hexDistance(Vector2 a, Vector2 b)
        { return hexDistance((int)a.x, (int)a.y, (int)b.x, (int)b.y); }

    private bool IsInMoveRange(UnitData unit, Vector2 destination)
    {
        return (hexDistance(unit.Position, destination) <= unit.Speed);
    }
}
