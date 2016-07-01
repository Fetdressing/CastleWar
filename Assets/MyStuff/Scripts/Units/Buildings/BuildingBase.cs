using UnityEngine;
using System.Collections;

public class BuildingBase : AIBase {
    [HideInInspector]
    public int attackedCount = 1000000000; //så den inte kallar på hjälp hela tiden
	// Use this for initialization
	void Start () {
        Init();
	}

    public override void Attacked(Transform attacker)
    {
        base.Attacked(attacker);
        attackedCount++;
        if(attackedCount > 3) //performance bara
        {
            NotifyNearbyFriendly(attacker.position);
            attackedCount = 0;
        }
    }
}
