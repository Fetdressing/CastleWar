using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AbilityProjectile : CastAbility {
    public GameObject projectile;
    [HideInInspector]
    public List<GameObject> projectilePool = new List<GameObject>();
    public int projectilePoolSize = 4;

    public int baseDamage = 40;

    public int projectileLifeTime = 3;
    // Use this for initialization

    public override void InitAbility(Transform caster)
    {
        base.InitAbility(caster);
        for (int i = 0; i < projectilePoolSize; i++)
        {
            GameObject tempO = Instantiate(projectile.gameObject) as GameObject;
            tempO.GetComponent<Projectile>().Init(eLayers, fLayers, thisTransform);
            projectilePool.Add(tempO.gameObject);
        }
    }

    public override void Dealloc()
    {
        base.Dealloc();
        for (int i = 0; i < projectilePoolSize; i++)
        {
            if (projectilePool[i] != null)
            {
                projectilePool[i].GetComponent<Projectile>().Dealloc();
                Destroy(projectilePool[i].gameObject);
            }
        }
    }


    public override void ApplyEffect()
    {
        base.ApplyEffect();
        GameObject readyProjectile = null;
        Projectile lastProjectileScript = null;
        for (int i = 0; i < projectilePool.Count; i++)
        {
            lastProjectileScript = projectilePool[i].GetComponent<Projectile>();
            if (lastProjectileScript.IsReady())
            {
                readyProjectile = projectilePool[i].gameObject;
                break;
            }
        }
        if (readyProjectile != null) //fire
        {
            readyProjectile.transform.position = thisTransform.position;

            if(validTargets == ValidTargets.Allied || validTargets == ValidTargets.Both) //friendly fire
            {
                lastProjectileScript.Fire(thisTransform, targetPosition, baseDamage, projectileLifeTime, true, true);
            }
            else
            {
                lastProjectileScript.Fire(thisTransform, targetPosition, baseDamage, projectileLifeTime, true, false);
            }

        }
    }
}
