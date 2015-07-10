using UnityEngine;
using System.Collections;

public class Hex : MonoBehaviour {

    public UnitData Unit;
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
        Debug.Log("Click at" + Position);
        GetComponentInParent<BattleEngine>().MovePiece(Position);

    }


    public override int GetHashCode()
    {
        return (int)Position.x * 7 + (int)Position.y * 13;
    }
}
