using UnityEngine;
using System.Collections;

public class Hex : MonoBehaviour {

    public UnitData Unit;
    public bool HasObstacle;
    public Vector2 Position;

    public void LogClick()
    {
        Debug.Log("Click at" + Position);
        GetComponentInParent<BattleEngine>().MovePiece(Position);
    }
}
