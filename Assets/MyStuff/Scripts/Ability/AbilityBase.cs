using UnityEngine;
using System.Collections;

[System.Serializable]
public class AbilityBase : MonoBehaviour{
    public enum ValidTargets { Allied, Enemy, Both };


    public string name;
    [HideInInspector]
    public float aoe = 0;
    public ValidTargets validTargets = ValidTargets.Enemy;

    public virtual void InitAbility(Transform caster)
    {

    }

    public virtual void ApplyEffect()
    {

    }
}
