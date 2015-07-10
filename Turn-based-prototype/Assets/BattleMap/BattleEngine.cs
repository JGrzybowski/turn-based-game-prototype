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


    //MOVEMENT
    public bool MoveUnit(Vector2 from, Vector2 to)
    {
        UnitData unit = ((Hex)grid[from]).Unit;
        if (!IsLandable(to))
            return false;
        if (calculateDistance(from, to) > unit.Speed)
            return false;

        MoveUnit(unit, to);
        return true;
    }
    public void MovePiece(Vector2 position)
    {
        MoveUnit(unit.GetComponent<UnitData>(), position);
    }
    public void MoveUnit(UnitData unit, Vector2 position)
    {
        unit.Position = position;
        unit.transform.SetParent((grid[position]).transform);
        unit.GetComponent<RectTransform>().position = unit.GetComponentInParent<Hex>().transform.position;
    }

    public void SpawnExampleUnit()
    {
        unit = (GameObject)Instantiate(ExampleUnit, Vector3.zero, Quaternion.identity);
        unit.transform.SetParent(grid.transform);
        MovePiece(new Vector2(0, 0));
        unit.GetComponent<RectTransform>().localScale = Vector3.one;
    }

    private int calculateDistance(Vector2 from, Vector2 to)
    {
        throw new NotImplementedException();
    }
}
