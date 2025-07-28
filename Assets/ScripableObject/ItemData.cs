using System;
using UnityEngine;

[Serializable]
public class ItemData
{
    public StatBlock PlayerHP;
    public StatBlock EnemyHP_Easy;
    public StatBlock EnemyHP_Normal;
    public StatBlock EnemyHP_Hard;
    public StatBlock NormalAttack;
    public StatBlock SmallAttack;
    public StatBlock PowerThrow;
    public StatBlock DoubleAttack;
    public StatBlock Heal;
    public StatBlock TimeToThink;
    public StatBlock TimeToWarning;
    public StatBlock WindValues;
    public StatBlock NormalEnemyActivate;
    public StatBlock HardEnemyActivate;
}
