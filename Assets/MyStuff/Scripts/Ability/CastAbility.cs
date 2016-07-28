using UnityEngine;
using System.Collections;

public class CastAbility : AbilityBase {
    [HideInInspector]
    public Vector3 targetPosition;

    public virtual int CastSpell(Vector3 targetPos, int currFatigue, ref bool isCastable) //returnerar kostnaden av spellen
    {

        if (!IsReady(currFatigue))
        {
            isCastable = false;
            return 0;
        }

        if(Vector3.Distance(thisTransform.position, targetPos) > range)
        {
            return 0;
        }
        targetPosition = targetPos;
        cooldown_Timer = Time.time + cooldown;
        ApplyEffect();
        return fatigueCost;
    }
}
