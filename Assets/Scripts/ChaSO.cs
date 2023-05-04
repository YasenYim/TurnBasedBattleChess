using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitState
{
    Idle,
    Moved,
    Attack,
    Finish,
    Dead,
}

[CreateAssetMenu(fileName ="new cha", menuName ="Design Data2/Cha2")]
public class ChaSO : ScriptableObject
{
    public int cha_id;
    public string cha_name;
    public int move;
    public bool move_attack;
    public int attack_range_low;
    public int attack_range_high;

    public int hp;
    public int attack;
    public GameObject particle;
    public GameObject bullet;
    public UnitState chaState;

    [Tooltip("None, 草, 桥, 水, 山")]
    public List<int> move_cost;
}
