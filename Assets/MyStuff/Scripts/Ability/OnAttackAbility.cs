using UnityEngine;
using System.Collections;

[System.Serializable]
public class OnAttackAbility : AbilityBase {

    public virtual int ExecuteAttackModifier(Transform target, float range, int currFatigue) //returnerar kostnaden av spellen
    {
        if (cooldown_Timer >= Time.time)
        {
            return 0;
        }
        cooldown_Timer = Time.time + cooldown;
        base.ApplyEffect();
        return fatigueCost;
    }

}
