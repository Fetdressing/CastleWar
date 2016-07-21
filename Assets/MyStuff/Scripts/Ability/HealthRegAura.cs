using UnityEngine;
using System.Collections;

public class HealthRegAura : PassiveAbility {
    public int regAmount = -10;
    // Use this for initialization
    public override void ApplyEffect()
    {
        if (isInit == false) return;
        base.ApplyEffect();
        Transform[] targets = ScanTargets(aoe);
        if (targets == null)
            return;
        for(int i = 0; i < targets.Length; i++)
        {
            targets[i].GetComponent<Health>().ApplyBuffHealthReg(regAmount, duration, casterT, casterIDName+name);
        }
    }
}
