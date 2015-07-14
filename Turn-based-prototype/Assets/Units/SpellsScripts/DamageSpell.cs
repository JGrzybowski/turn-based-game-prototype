using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class DamageSpell : CircularAreaSpell
{
    public int BaseDamage;
    public DamageType DamageType;

    public override void Apply(UnitBase caster, UnitBase unit)
    {
        unit.Damage(caster, this.BaseDamage * caster.NumberOfUnits, this.DamageType, AttackType.Spell);
    }
}

