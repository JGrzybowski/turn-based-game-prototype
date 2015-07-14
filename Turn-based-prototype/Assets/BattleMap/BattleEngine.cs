using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
public enum UIMode { Walk, WalkThenAttack, Spell}
public class BattleEngine : MonoBehaviour {

    [SerializeField]
	private HexBoard grid;
    [SerializeField]
    private UnitBase activeUnit;

    private UIMode uiMode;
    [SerializeField]
    private GameObject SkillButton;
    
    private UnitBase targetUnit;
    public SpellBase activeSpell;

    public UnitBase ActiveUnit
    {
        get { return activeUnit;}
        set
        {
            markReachableHexes(ActiveUnit,value);
            activeUnit = value;
            if(value != null)
                UpdateUI(ActiveUnit);
        }
    }
    
    public int attDefBalanceConstant = 5;

    private Vector2 positionToClean;
    private bool inWaitingTurn = false;

    public GameObject[] InitialUnits;
    public Queue<UnitBase> ThisTurnQueue = new Queue<UnitBase>();
    public Queue<UnitBase> WaitingQueue = new Queue<UnitBase>();
    public Queue<UnitBase> NextTurnQueue = new Queue<UnitBase>();
    public List<UnitBase> AllUnits { get { return ThisTurnQueue.Union(WaitingQueue).Union(NextTurnQueue).ToList(); } }

    public void SetSpellMode()
    {
        uiMode = UIMode.Spell;
    }

    private void Start()
    {
        for (int i= 0; i< InitialUnits.Count(); i++)
        {
            ThisTurnQueue.Enqueue(SpawnExampleUnit(InitialUnits[i]));
        }
        ActivateNextUnit(null);
    }

    public void HexClicked(Vector2 position)
    {
        switch (uiMode)
        {
            case UIMode.Walk:
                MoveUnit(position);
                break;
            case UIMode.WalkThenAttack:
                MoveUnit(position);
                AttackUnit(targetUnit);
                break;
            case UIMode.Spell:
                if (activeSpell.Target == SpellTarget.Position)
                    RunSpell(position);
                break;
            default:
                break;
        }
    }
    public void UnitClicked(UnitBase unit)
    {
        switch (uiMode)
        {
            case UIMode.Walk:
                AttackUnit(unit);
                break;
            case UIMode.WalkThenAttack:
                break;
            case UIMode.Spell:
                if (activeSpell.Target == SpellTarget.Position)
                    RunSpell(unit.Position);
                else if (activeSpell.Target == SpellTarget.Enemy && ActiveUnit.Player != unit.Player)
                    RunSpell(unit.Position);
                else if (activeSpell.Target == SpellTarget.Ally && ActiveUnit.Player == unit.Player)
                    RunSpell(unit.Position);
                break;
            default:
                break;
        }
    }

    public void WaitUnit()
    {
        if (!inWaitingTurn)
        {
            positionToClean = ActiveUnit.Position;
            ActivateNextUnit(WaitingQueue);
        }
    }
    public void AttackUnit(UnitBase unit)
    {
        if (dealDamage(ActiveUnit, unit))
        {
            positionToClean = ActiveUnit.Position;
            ActivateNextUnit(NextTurnQueue);
        }
    }
    public void MoveUnit(Vector2 to)
    {
        if (MoveUnit(ActiveUnit.GetComponent<UnitBase>(), to))
        {
            string msg = string.Format("{0} moved to {1},{2}.", ActiveUnit.Name, ActiveUnit.Position.x, ActiveUnit.Position.y);
            Debug.Log(msg);
            ActivateNextUnit(NextTurnQueue);
        }
    }

    private void RunSpell(Vector2 target)
    {
        var area = activeSpell.Area;
        area = area.ConvertAll(offset => offset + target);
        area = area.Where(position => grid.isOnBoard(position)).ToList();
        var affectedUnits = AllUnits.Where(unit => area.Any(position => position == unit.Position));
        activeSpell.CooldownTimer = activeSpell.CoolDown;
        foreach (var unit in affectedUnits)
        {
            activeSpell.Apply(ActiveUnit, unit);
        }
        
        ActivateNextUnit(NextTurnQueue);
    }

    //Dealing Damage
    private bool dealDamage(UnitBase attacker, UnitBase defender)
    {
        if (!isInRange(attacker,defender.Position,attacker.AttackRange) || attacker.Player == defender.Player)
            return false;

        int distance = hexDistance(attacker.Position, defender.Position);
        if (distance > 1)
            attacker.AttackRanged(defender);
        else
            attacker.AttackMeele(defender);

        return true;
    }

    public delegate void OnModeChangeHandler();

    public void UpdateUI(UnitBase unit)
    {
        if (unit.Spells.Length > 0 && unit.Spells[0].CooldownTimer == 0 && unit.Mana >= unit.Spells[0].ManaCost)
        {
            SkillButton.SetActive(true);
            SkillButton.GetComponent<UnityEngine.UI.Image>().sprite = unit.Spells[0].Icon;
            this.activeSpell = unit.Spells[0];
        }
        else
        {
            SkillButton.SetActive(false);
        }

        //SkillButton.image = ActiveUnit.Skills

    }

    //MOVEMENT
    private void ActivateNextUnit(Queue<UnitBase> queueToJoin)
    {
        if(ActiveUnit != null && ActiveUnit.Health > 0)
            queueToJoin.Enqueue(ActiveUnit);

        ThisTurnQueue = new Queue<UnitBase>(ThisTurnQueue.OrderByDescending(unit => unit.Initiative));
                        
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
            AllUnits.ForEach(unit => unit.UpdateAfterTurnsEnd());
            ActivateNextUnit(null);
        }
        uiMode = UIMode.Walk;
    }
    private bool MoveUnit(UnitBase unit, Vector2 to)
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
    private void setUnitPosition(UnitBase unit, Vector2 position)
    {
        unit.transform.SetParent((grid[position]).transform);
        unit.GetComponent<RectTransform>().position = unit.GetComponentInParent<Hex>().transform.position;
    }
    
    public UnitBase SpawnExampleUnit(GameObject prefab)
    {
        var obj = (GameObject)Instantiate(prefab, Vector3.zero, Quaternion.identity);
        var unit = obj.GetComponent<UnitBase>();
        setUnitPosition(unit, unit.Position);
        obj.GetComponent<RectTransform>().localScale = Vector3.one;
        return unit;
    }


    private void markReachableHexes(UnitBase previousUnit, UnitBase nextUnit)
    {
        if (this.positionToClean != null && previousUnit != null)
        {
            foreach (var position in grid.ProperHexesInRange(this.positionToClean, previousUnit.Speed))
            {
                grid[position].IsInMoveRange = false;
            }
        }
        if (nextUnit != null)
        {
            foreach (var position in grid.ProperHexesInRange(nextUnit.Position, nextUnit.Speed))
            {
                grid[position].IsInMoveRange = true;
            }
        }
    }
    private int hexDistance(int qa, int ra, int qb,int rb)
        { return (Math.Abs(qa - qb) + Math.Abs(ra - rb) + Math.Abs(qa + ra - qb - rb)) / 2; }
    private int hexDistance(Vector2 a, Vector2 b)
        { return hexDistance((int)a.x, (int)a.y, (int)b.x, (int)b.y); }

    private bool isInRange(UnitBase unit, Vector2 destination, int range)
    {
        return (hexDistance(unit.Position, destination) <= range);
    }
    public void RemoveUnit(UnitBase unit)
    {
        List<UnitBase> unitsToRemove = new List<UnitBase> { unit };
        if (this.ActiveUnit == unit)
            this.ActiveUnit = null;
        ThisTurnQueue = new Queue<UnitBase>(ThisTurnQueue.Except(unitsToRemove));
        WaitingQueue = new Queue<UnitBase>(WaitingQueue.Except(unitsToRemove));
        NextTurnQueue = new Queue<UnitBase>(NextTurnQueue.Except(unitsToRemove));
        Destroy(unit.gameObject);
    }
    private bool IsLandable(Vector2 place)
        { return !(grid[place]).HasObstacle; }

}
