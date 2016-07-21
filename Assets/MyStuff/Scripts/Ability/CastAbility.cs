using UnityEngine;
using System.Collections;

public class CastAbility : AbilityBase {

    public virtual int CastSpell(Transform target, float range, int currFatigue) //returnerar kostnaden av spellen
    {
        if(cooldown_Timer >= Time.time)
        {
            return 0;
        }
        cooldown_Timer = Time.time + cooldown;
        base.ApplyEffect();
        return fatigueCost;
    }
}
