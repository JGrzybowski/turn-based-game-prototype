﻿using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
public enum UIMode { Walk, WalkThenAttack, Spell}
public class BattleEngine : MonoBehaviour {

    [SerializeField]
	private HexBoard grid;
    //TODO Walk to enemy and attack
    private UnitBase targetUnit;
    [SerializeField]
    private GameObject waitButton;
    [SerializeField]
    private GameObject skillButton;
    public int attDefBalanceConstant = 5;

    private UIMode uiMode;
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
    [SerializeField]
    private UnitBase activeUnit;
    public SpellBase ActiveSpell;

    private Vector2 positionToClean;
    private bool inWaitingTurn = false;
    
    //Queues and unit lists
    public GameObject[] InitialUnits;
    public Queue<UnitBase> ThisTurnQueue = new Queue<UnitBase>();
    public Queue<UnitBase> WaitingQueue = new Queue<UnitBase>();
    public Queue<UnitBase> NextTurnQueue = new Queue<UnitBase>();
    public List<UnitBase> AllUnits {
        get
        {
            var list = ThisTurnQueue.Union(WaitingQueue).Union(NextTurnQueue).ToList();
            if(ActiveUnit != null)
                list.Add(ActiveUnit);
            return list;
        }
    }
    
    //Initial functions
    private void Start()
    {
        for (int i= 0; i< InitialUnits.Count(); i++)
        {
            ThisTurnQueue.Enqueue(SpawnExampleUnit(InitialUnits[i]));
        }
        ActivateNextUnit(null);
    }
    public UnitBase SpawnExampleUnit(GameObject prefab)
    {
        var obj = (GameObject)Instantiate(prefab, Vector3.zero, Quaternion.identity);
        var unit = obj.GetComponent<UnitBase>();
        setUnitPosition(unit, unit.Position);
        obj.GetComponent<RectTransform>().localScale = Vector3.one;
        return unit;
    }

    //UI related functions
    public void SetSpellMode()
    {
        uiMode = UIMode.Spell;
    }
    public void UpdateUI(UnitBase unit)
    {
        if (unit.Spells.Length > 0 && unit.Spells[0].CooldownTimer == 0 && unit.Mana >= unit.Spells[0].ManaCost)
        {
            skillButton.SetActive(true);
            skillButton.GetComponent<UnityEngine.UI.Image>().sprite = unit.Spells[0].Icon;
            this.ActiveSpell = unit.Spells[0];
        }
        else
        {
            skillButton.SetActive(false);
        }
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

    //Functions to start units actions
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
                if (ActiveSpell.Target == SpellTarget.Position)
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
                if (ActiveSpell.Target == SpellTarget.Position)
                    RunSpell(unit.Position);
                else if (ActiveSpell.Target == SpellTarget.Enemy && ActiveUnit.Player != unit.Player)
                    RunSpell(unit.Position);
                else if (ActiveSpell.Target == SpellTarget.Ally && ActiveUnit.Player == unit.Player)
                    RunSpell(unit.Position);
                break;
            default:
                break;
        }
    }

    //Active Unit actions
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
        var area = ActiveSpell.Area;
        area = area.ConvertAll(offset => offset + target);
        area = area.Where(position => grid.isOnBoard(position)).ToList();
        var affectedUnits = AllUnits.Where(unit => area.Any(position => position == unit.Position));
        ActiveSpell.CooldownTimer = ActiveSpell.CoolDown;
        foreach (var unit in affectedUnits)
        {
            ActiveSpell.Apply(ActiveUnit, unit);
        }
        this.positionToClean = ActiveUnit.Position;
        ActivateNextUnit(NextTurnQueue);

    }

    //Dealing damage and removing units
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
    
    //Picks next unit from the queue
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
            waitButton.SetActive(false);
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
            waitButton.SetActive(true);
        }
        uiMode = UIMode.Walk;
    }

    //Sub-functions for active unit actions
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
    
    //Distance calculations
    private int hexDistance(int qa, int ra, int qb,int rb)
        { return (Math.Abs(qa - qb) + Math.Abs(ra - rb) + Math.Abs(qa + ra - qb - rb)) / 2; }
    private int hexDistance(Vector2 a, Vector2 b)
        { return hexDistance((int)a.x, (int)a.y, (int)b.x, (int)b.y); }
    private bool isInRange(UnitBase unit, Vector2 destination, int range)
    {
        return (hexDistance(unit.Position, destination) <= range);
    }
    private bool IsLandable(Vector2 place)
        { return !(grid[place]).HasObstacle; }
}
