using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InputState
{
    Wait,       // �ȴ���������������
    Ready,
    SelectUnit,
    UnitMove,
    UnitAction,
}

public class StateWait : GameState<InputState>
{
    public override void OnStateEnter() { } 
    public override void OnStateLogic() { } 
    public override void OnStateExit() { }
}

public class StateReady : GameState<InputState>
{
    public override void OnStateEnter() { } 
    public override void OnStateLogic() { } 
    public override void OnStateExit() { }
}

public class StateSelectUnit : GameState<InputState>
{
    public override void OnStateEnter()
    {
        var gm = GameManager.Instance;
        gm.curUnit = null;
        MapManager.Instance.HideAllPaths();
        gm.ShowEndRoundButton(true);
    }

    public override void OnStateLogic()
    {
        var gm = GameManager.Instance;
        Unit unit = gm.GetUnit(gm.pos);
        if (unit == null) { return; }
        //Debug.Log("ѡȡ��Ŀ��" + unit.chaData.cha_name);
        if (unit.army != gm.curArmyIndex) { return; }
        if (unit.state != UnitState.Idle) { return; }
        gm.curUnit = unit;
        // չʾ�ƶ���Χ
        fsm.State = InputState.UnitMove;
    }

    public override void OnStateExit()
    {
        var gm = GameManager.Instance;
        gm.ShowEndRoundButton(false);
    }
}

public class StateUnitMove : GameState<InputState>
{
    public override void OnStateEnter()
    {
        var gm = GameManager.Instance;
        gm.curMoveArea = MapManager.Instance.CalcMoveArea(gm.curUnit);
        MapManager.Instance.ShowBluePaths(gm.curMoveArea);
    }

    public override void OnStateLogic()
    {
        var gm = GameManager.Instance;
        Unit tempUnit = gm.GetUnit(gm.pos);
        if (tempUnit != null && tempUnit != gm.curUnit)
        {
            // ѡ����������λ
            //Debug.Log("ѡȡ��Ŀ��" + unit.chaData.cha_name);
            if (tempUnit.army != gm.curArmyIndex || tempUnit.state != UnitState.Idle)
            {
                fsm.State = InputState.SelectUnit;
                return;
            }

            gm.curUnit = tempUnit;
            fsm.State = InputState.UnitMove;
            return;
        }
        if (gm.pos.Equals(gm.curUnit.pos))
        {
            // �ٴ�ѡ�����Լ��������ƶ�����
            // ���浱ǰλ�ã�����֮�����
            gm.beforeMovedPos = gm.curUnit.pos;
            // �ɹ�����Χ
            fsm.State = InputState.UnitAction;
            return;
        }
        // ѡ����λ���Ƿ����ƶ���Χ�������ƶ�����Ϊ-1
        int targetStep = gm.curMoveArea[gm.pos.y, gm.pos.x];
        if (targetStep < 0)
        {
            fsm.State = InputState.SelectUnit;
            return;
        }

        // curUnit��Ŀ����ƶ�
        gm.curUnit.state = UnitState.Moved;
        List<Pos> list = MapManager.Instance.CalcWay(
            gm.curUnit.pos, gm.pos, gm.curMoveArea);
        Sequence seq = DOTween.Sequence();
        for (int i = 1; i < list.Count; i++)
        {
            Ease ease = Ease.Linear;
            float time = 0.2f;
            if (i == 1)
            {
                ease = Ease.InSine;
                time = 0.28f;
            }
            else if (i == list.Count - 1)
            {
                ease = Ease.OutSine;
                time = 0.28f;
            }

            Vector3 dir = list[i].ToVec3() - list[i - 1].ToVec3();
            var tw2 = gm.curUnit.transform.DOLookAt(dir * 10000, 0.01f);
            seq.Append(tw2);

            var tw = gm.curUnit.transform.DOMove(list[i].ToVec3(), time).SetEase(ease);
            seq.Append(tw);
            gm.curUnit.isRunning = true;
        }

        fsm.State = InputState.Wait;
        seq.AppendCallback(() => {
            gm.curUnit.isRunning = false;
            // ����λ��
            gm.beforeMovedPos = gm.curUnit.pos;
            gm.allUnits[gm.curUnit.pos.y, gm.curUnit.pos.x] = null;
            gm.curUnit.pos = gm.pos;
            gm.allUnits[gm.curUnit.pos.y, gm.curUnit.pos.x] = gm.curUnit;

            fsm.State = InputState.UnitAction;
        });
    }

    public override void OnStateExit()
    {
        MapManager.Instance.HideBluePaths();
    }
}


public class StateUnitAction : GameState<InputState>
{
    public override void OnStateEnter()
    {
        var gm = GameManager.Instance;
        gm.ShowEndActButton(true);
        gm.attackArea = new List<Pos>();
        if (gm.curUnit.state != UnitState.Moved || gm.curUnit.chaData.move_attack)
        {
            gm.attackArea = MapManager.Instance.CalcAttackPos(gm.curUnit);
            MapManager.Instance.ShowRedPaths(gm.attackArea);
        }
    }

    public override void OnStateLogic()
    {
        var gm = GameManager.Instance;
        if (!gm.attackArea.Contains(gm.pos))
        {
            // ȡ���ж����ص��ƶ��׶�
            // ��ԭλ��
            gm.curUnit.state = UnitState.Idle;

            gm.allUnits[gm.curUnit.pos.y, gm.curUnit.pos.x] = null;
            gm.curUnit.pos = gm.beforeMovedPos;
            gm.allUnits[gm.curUnit.pos.y, gm.curUnit.pos.x] = gm.curUnit;
            gm.curUnit.transform.position = gm.curUnit.pos.ToVec3();

            fsm.State = InputState.SelectUnit;
            return;
        }

        // ����Ŀ��λ��
        Unit u1 = gm.curUnit;
        Unit u2 = gm.allUnits[gm.pos.y, gm.pos.x];

        if (u2 == null)
        {
            return;
        }
        if (u2 == u1)
        {
            // todo: �������ͷŵļ���
            return;
        }
        // ����
        if (u2.army == u1.army)
        {
            // todo: �ж��ѷ�����
            return;
        }

        Vector3 dir = gm.pos.ToVec3() - u1.transform.position;
        u1.transform.LookAt(u1.transform.position + dir);
        u1.isAttack = true;

        MapManager.Instance.HideAllPaths();

        // ʱ�����⣺Ҫ�ȴ������壬���ٹ�һ֡�ٵ��ú����������ڴ���ʱ�����������ĺ���
        if (!u2.costHpNum)
        {
            CostHpNum c = GameObject.Instantiate(gm.prefabCostHp, null);
            u2.costHpNum = c;
        }
        fsm.State = InputState.Wait;
        u1.state = UnitState.Finish;

        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(0.5f);
        seq.AppendCallback(() => {
            u1.Attack(u2);
            if (u2.state == UnitState.Dead)
            {
                gm.allUnits[u2.pos.y, u2.pos.x] = null;
            }
        });
        seq.AppendInterval(0.8f);
        seq.AppendCallback(() => {
            gm.curUnit.EndAction();
            fsm.State = InputState.SelectUnit;
        });
    }

    public override void OnStateExit()
    {
        var gm = GameManager.Instance;
        gm.attackArea = null;
        MapManager.Instance.HideRedPaths();
        gm.ShowEndActButton(false);
    }
}

