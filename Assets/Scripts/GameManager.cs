using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Army
{
    public List<Unit> units;
}

public class GameManager : MonoBehaviour
{
    public List<Army> armies;
    [SerializeField]
    Button btnEndAct;
    [SerializeField]
    Button btnEndRound;

    public CostHpNum prefabCostHp;

    [HideInInspector]
    public int curArmyIndex = 0;
    [HideInInspector]
    public Unit curUnit;
    public Pos beforeMovedPos;

    public int[,] curMoveArea;
    public List<Pos> attackArea;

    public Unit[,] allUnits;

    public Transform imgRoundA;
    public Transform imgRoundB;

    // 当前选定的位置
    public Pos pos;
    FSM<InputState> fsm;

    public static GameManager Instance { get; private set; }


    private void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        Init();
        fsm = new FSM<InputState>(InputState.SelectUnit);
        fsm.RegisterState(InputState.Ready, new StateReady());
        fsm.RegisterState(InputState.SelectUnit, new StateSelectUnit());
        fsm.RegisterState(InputState.UnitMove, new StateUnitMove());
        fsm.RegisterState(InputState.UnitAction, new StateUnitAction());
        fsm.RegisterState(InputState.Wait, new StateWait());
        ShowEndActButton(false);
    }

    public void Init()
    {
        allUnits = new Unit[MapManager.Instance.H, MapManager.Instance.W];
        for (int i=0; i<armies.Count; i++)
        {
            Army army = armies[i];
            foreach (var unit in army.units)
            {
                Pos p = new Pos(unit.transform.position);
                unit.pos = p;
                unit.army = i;
                allUnits[p.y, p.x] = unit;
            }
        }

        for (int i = 0; i < armies.Count; i++)
        {
            Army army = armies[i];
            foreach (var unit in army.units)
            {
                Pos p = new Pos(unit.transform.position);
                unit.pos = p;
                unit.army = i;
                unit.state = UnitState.Idle;
            }
        }
    }

    public void InitRound()
    {
        for (int i = 0; i < armies.Count; i++)
        {
            Army army = armies[i];
            foreach (var unit in army.units)
            {
                Pos p = new Pos(unit.transform.position);
                if (unit.state == UnitState.Finish)
                {
                    unit.state = UnitState.Idle;
                    unit.GetComponentInChildren<SkinnedMeshRenderer>().material.SetColor("_Color", Color.white);
                }
            }
        }
    }

    public void OnTouch(Pos pos, bool ok, bool cancel)
    {
        if (ok)
        {
            this.pos = pos;
            fsm.OnStateLogic();
        }
    }

    public Unit GetUnit(Pos p)
    {
        Unit unit = allUnits[p.y, p.x];
        return unit;
    }

    public void ShowEndRoundButton(bool visible)
    {
        btnEndRound.gameObject.SetActive(visible);
    }

    public void ShowEndActButton(bool visible)
    {
        btnEndAct.gameObject.SetActive(visible);
    }

    public void EndAct()
    {
        // 单位行动结束
        if (fsm.State != InputState.UnitAction)
        {
            return;
        }
        if (curUnit == null)
        {
            return;
        }
        curUnit.state = UnitState.Finish;
        curUnit.EndAction();
        curUnit = null;
        fsm.State = InputState.SelectUnit;
    }

    public void EndRound()
    {
        fsm.State = InputState.Wait;
        var imgRound = imgRoundB;
        if (curArmyIndex == 1)
        {
            imgRound = imgRoundA;
        }
        Vector3 orig = imgRound.position;
        GameObject canvas = GameObject.Find("Canvas");
        Vector3 center = canvas.transform.Find("Center").position;

        imgRound.gameObject.SetActive(true);
        Sequence seq = DOTween.Sequence();
        seq.Append(imgRound.DOMove(center, 0.2f));
        seq.AppendInterval(0.6f);
        seq.Append(imgRound.DOMove(new Vector3(-orig.x, orig.y, orig.z), 0.25f));
        seq.AppendCallback(() =>
        {
            imgRound.gameObject.SetActive(false);
            imgRound.position = orig;

            // start other side
            curArmyIndex++;
            if (curArmyIndex == 2) { curArmyIndex = 0; }
            InitRound();
            fsm.State = InputState.SelectUnit;
        });
    }
}

