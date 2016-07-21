using UnityEngine;
using System.Collections;
[System.Serializable]
public class PassiveAbility : AbilityBase {
    public static float interval = 0.5f;

    public override void InitAbility(Transform caster)
    {
        base.InitAbility(caster);
        GameObject temp = Instantiate(this.gameObject, caster.position, Quaternion.identity) as GameObject;
        temp.transform.SetParent(caster);

        StartCoroutine(PassiveUpdate());        
    }

    IEnumerator PassiveUpdate()
    {
        while(this != null)
        {
            ApplyEffect();
            yield return new WaitForSeconds(interval);
        }
    }

    public virtual void ApplyEffect()
    {

    }
}
