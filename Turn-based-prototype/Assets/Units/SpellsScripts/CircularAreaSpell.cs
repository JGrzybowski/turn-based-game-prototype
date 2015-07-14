using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public abstract class CircularAreaSpell : SpellBase
{
    public int AreaRange;

    public override List<Vector2> Area { get { return HexBoard.HexesInRange(new Vector2(0, 0), this.AreaRange); } }
   
}
