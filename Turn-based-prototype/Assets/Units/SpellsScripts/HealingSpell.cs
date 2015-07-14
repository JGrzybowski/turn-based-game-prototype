using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class HealingSpell : CircularAreaSpell
{
    public int BaseHeal;
    public override void Apply(UnitBase caster, UnitBase unit)
    {
        unit.Heal(caster.NumberOfUnits * this.BaseHeal);
    }
}

