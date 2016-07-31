using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HealthRegAura : PassiveAbility {
    public int regAmount = -10;
    // Use this for initialization
    public override void ApplyEffect()
    {
        if (initTimes == 0) return;
        base.ApplyEffect();
        Transform[] targets = ScanTargets(aoe);
        if (targets == null)
            return;
        for(int i = 0; i < targets.Length; i++)
        {
            //targets[i].GetComponent<Health>().ApplyBuffHealthReg(regAmount, duration, casterT, casterIDName+name);
            targets[i].GetComponent<Health>().ApplyBuff(StatType.HealthReg,name ,regAmount, duration, casterT, casterIDName, doesStack);
        }
    }

    public override List<int> ApplyToolTipValues()
    {
        List<int> indexes = base.ApplyToolTipValues();
        int offset = 0;
        if (indexes.Count > 0)
        {
            for (int i = 0; i < indexes.Count; i++)
            {
                string stringToAdd = regAmount.ToString();
                tooltip = tooltip.Insert(indexes[i] + offset, stringToAdd);
                //Debug.Log(tooltip.Length.ToString());
                offset += stringToAdd.Length;
            }
        }
        tooltip = tooltip.Replace('%', ' ');
        return indexes;
    }
}
