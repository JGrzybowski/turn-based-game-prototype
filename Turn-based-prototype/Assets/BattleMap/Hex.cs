using UnityEngine;
using System.Collections;

public class Hex : MonoBehaviour {

    public UnitDataBase Unit;
    public bool HasObstacle;
    public Vector2 Position;

    private bool isInMoveRange = false;
    public bool IsInMoveRange
    {
        get { return isInMoveRange; }
        set
        {
            isInMoveRange = value;
            GetComponent<Animator>().SetBool("IsInMoveRange", value);
        }
    }

    private void Awake()
    {
        GetComponent<Animator>().SetBool("IsInMoveRange", false);
    }

    public void LogClick()
    {
        GetComponentInParent<BattleEngine>().MoveUnit(Position);
    }


    public override int GetHashCode()
    {
        return (int)Position.x * 7 + (int)Position.y * 13;
    }

}
