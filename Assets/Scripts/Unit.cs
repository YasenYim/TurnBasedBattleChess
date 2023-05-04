using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public ChaSO chaData;
    [HideInInspector]
    public UnitState state;
    [HideInInspector]
    public Pos pos;
    [HideInInspector]
    public int army;

    public CostHpNum costHpNum;

    Animator anim;
    public bool isRunning;
    public bool isAttack;
    public bool isGetHit;
    public bool isDead;

    public int hp;

    void Start()
    {
        anim = GetComponent<Animator>();
        hp = chaData.hp;
    }

    void Update()
    {
        UpdateAnim();
    }

    void UpdateAnim()
    {
        anim.SetBool("Run", isRunning);
        if (isAttack)
        {
            anim.SetTrigger("Attack");
            isAttack = false;
        }
        if (isGetHit)
        {
            anim.SetTrigger("GetHit");
            isGetHit = false;
        }
        anim.SetBool("Death", isDead);
    }

    public void Attack(Unit other)
    {
        if (other.costHpNum)
        {
            other.costHpNum.ShowHpNum(other.pos.ToVec3(), "-" + chaData.attack.ToString());
        }
        other.hp -= chaData.attack;
        if (other.hp <= 0)
        {
            other.hp = 0;
            other.isDead = true;
            other.state = UnitState.Dead;
        }
        else
        {
            other.isGetHit = true;
        }
    }

    public void EndAction()
    {
        state = UnitState.Finish;
        GetComponentInChildren<SkinnedMeshRenderer>().material.SetColor("_Color", Color.red);
    }
}
