using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AgentRanged : AgentBase {

    [Header("Ranged")]
    public GameObject projectile;
    [HideInInspector]
    public List<GameObject> projectilePool = new List<GameObject>();
    public int projectilePoolSize = 15;
    public Transform shooter;

    public float minimumTargetDistance = 10;
    public float projectileLifeTime = 4;

    // Use this for initialization
 //   void Start () {
 //       Init();
	//}

    void Awake()
    {
        //Init();
    }
	
	// Update is called once per frame
	void Update () {
        if (initializedTimes == 0)
            return;

        if (!healthS.IsAlive() && thisTransform.gameObject.activeSelf == false)
            return;

        UpdateEssentials();

        if (target != null)
        {
            targetDistance = GetTargetDistance();
        }

        switch (state)
        {
            case UnitState.Guarding:
                GuardingUpdate();
                break;

            case UnitState.AttackMoving:
                AttackMovingUpdate();
                break;

            case UnitState.Moving: //nått som kollar ifall jag kommit fram och isåfall vill jag nog vakta
                MovingUpdate();
                break;
            case UnitState.Investigating:
                InvestigatingUpdate();
                break;
            case UnitState.AttackingUnit:
                AttackUnitUpdate();
                break;
        }
    }

    public override void Reset()
    {
        base.Reset();

        //reset projectiles, för säkerhetsskull
        for (int i = 0; i < projectilePoolSize; i++)
        {
            projectilePool[i].GetComponent<Projectile>().Init(enemyLayers, friendlyLayers, thisTransform);
        }
    }


    public override void Init()
    {
        base.Init();

        for (int i = 0; i < friendlyLayers.Count; i++)
        {
            layerMaskLOSCheck |= (1 << LayerMask.NameToLayer(friendlyLayers[i])); //lägg till friendly layers
        }
        for (int i = 0; i < enemyLayers.Length; i++)
        {
            layerMaskLOSCheckFriendlyExcluded |= (1 << LayerMask.NameToLayer(enemyLayers[i]));
            layerMaskLOSCheck |= (1 << LayerMask.NameToLayer(enemyLayers[i])); //lägg till enemy layers
        }
        layerMaskLOSCheck |= (1 << LayerMask.NameToLayer("Terrain"));
        layerMaskLOSCheckFriendlyExcluded |= (1 << LayerMask.NameToLayer("Terrain"));


        if (initializedTimes > 1)
        {
            return;
        }
        for (int i = 0; i < projectilePoolSize; i++)
        {
            GameObject tempO = Instantiate(projectile.gameObject) as GameObject;
            tempO.GetComponent<Projectile>().Init(enemyLayers, friendlyLayers, thisTransform);
            projectilePool.Add(tempO.gameObject);
        }
        //layerMaskLOSCheckFriendlyExcluded = layerMaskLOSCheck; //set denna innan så att den får med alla friendly layers
        //for (int i = 0; i < friendlyLayers.Count; i++)
        //{
        //    layerMaskLOSCheck ^= (1 << LayerMask.NameToLayer(friendlyLayers[i])); //ta bort friendly layers
        //}

        //layerMaskLOSCheck |= (1 << friendlyOnly); //lägg till sin egen layer så att man kan skjuta igenom allierade
    }

    public override void Dealloc()
    {
        base.Dealloc();
        for (int i = 0; i < projectilePoolSize; i++)
        {
            projectilePool[i].GetComponent<Projectile>().Dealloc();
            Destroy(projectilePool[i].gameObject);
        }
    }

    public override bool AttackTarget()
    {
        bool targetAlive = true;
        if (target == null || target.gameObject.activeSelf == false || !targetHealth.IsAlive())
        {
            targetAlive = false;
            return targetAlive;
        }

        float targetDistanceuS = (targetDistance - targetHealth.unitSize);
        bool los, isFacingTarget;
        los = LineOfSight();
        isFacingTarget = IsFacingTransform(target);
        if (attackRange > targetDistanceuS && isFacingTarget) //kolla så att target står framför mig oxå
        {
            if (attackSpeedTimer <= Time.time)
            {
                if(los)
                {
                    attackSpeedTimer = attackSpeed + Time.time;
                    int damageRoll = RollDamage();
                    Fire(damageRoll); //den skjuter på marken, på fiende transformen, detta skulle kunna vara mer reliable
                }
            }
        }

        if (attackRange + 1 < targetDistanceuS || !los) //+1 för marginal
        {
            SetDestination(target.position);
        }
        else if(targetDistanceuS <= minimumTargetDistance)
        {
            Vector3 vectorFromTarget = thisTransform.position - target.position;
            SetDestination(thisTransform.position + vectorFromTarget); //gånger ett värde för att förflytta denne lite extra
        }
        else if (!isFacingTarget) //den resetar pathen för ovanstående när den går utanför minimumTargetDistance
        {
            RotateTowards(target);
            ResetPath();
        }
        else
        {
            ResetPath();
        }
        //ta bort "targetDistanceuS > (minimumTargetDistance - minimumTargetDistance * 0.3f)" från de två sista statementsen om det ej funkar
        return true;
    }

    public virtual void Fire(int damageRoll)
    {
        GameObject readyProjectile = null;
        Projectile lastProjectileScript = null;
        for(int i = 0; i < projectilePool.Count; i++)
        {
            lastProjectileScript = projectilePool[i].GetComponent<Projectile>();
            if (lastProjectileScript.IsReady())
            {
                readyProjectile = projectilePool[i].gameObject;
                break;
            }
        }
        if(readyProjectile != null) //fire
        {
            readyProjectile.transform.position = shooter.position;

            if (targetBase != null)
            {
                if (IsFriendly(target)) //om man har satt en friendly som target så måste man kunna skada den, så sätt ff = true
                {
                    lastProjectileScript.Fire(target, targetHealth.middlePoint, damageRoll, projectileLifeTime, true, true);
                }
                else
                {
                    lastProjectileScript.Fire(target, targetHealth.middlePoint, damageRoll, projectileLifeTime, true, false); //annars inte
                }
            }
            else //bara typ nån destructable
            {
                lastProjectileScript.Fire(target, targetHealth.middlePoint, damageRoll, projectileLifeTime, false, false);
            }
            
        }
    }

    public override bool LineOfSight() //has LOS to t?
    {
        RaycastHit hitLOS;
        //RaycastHit[] hitsLOS;
        Vector3 vectorToT = targetHealth.middlePoint - shooter.position; //hämta mittpunkten istället

        //List<Transform> potBlockers = new List<Transform>();

        //hitsLOS = Physics.RaycastAll(thisTransform.position, vectorToT, attackRange * 1.2f, layerMaskLOSCheck);

        //for(int i = 0; i < hitsLOS.Length; i++)
        //{
        //    if (hitsLOS[i].collider.gameObject.layer == LayerMask.NameToLayer("Terrain"))
        //    {
        //        potBlockers.Add(hitsLOS[i].collider.transform);
        //    }
        //}
        //return false;

        //if(Physics.SphereCast(thisTransform.position, losWidthCheck, vectorToT, out hitLOS, Mathf.Infinity, layerMaskLOSCheck)) 
        if (!isFriendlyTarget)
        {
            if (Physics.Raycast(shooter.position, vectorToT, out hitLOS, Mathf.Infinity, layerMaskLOSCheckFriendlyExcluded)) //ett layar som ignorerar allt förutom units o terräng
            {
                if (hitLOS.collider.gameObject.layer != LayerMask.NameToLayer("Terrain"))
                {
                    //Debug.Log(hitLOS.collider.transform.name);
                    return true;
                }
            }
        }
        else //friendly target
        {
            if (Physics.Raycast(shooter.position, vectorToT, out hitLOS, Mathf.Infinity, layerMaskLOSCheck)) //ett layar som ignorerar allt förutom units o terräng
            {
                if (hitLOS.collider.gameObject.layer != LayerMask.NameToLayer("Terrain"))
                {
                    //Debug.Log(hitLOS.collider.transform.name);
                    return true;
                }
            }
        }
        return false;
    }
    public override bool LineOfSight(Transform t) //has LOS to t?
    {
        RaycastHit hitLOS;
        Vector3 vectorToT = t.GetComponent<Health>().middlePoint - shooter.position; //hämta mittpunkten istället

        if (!IsFriendly(target))
        {
            if (Physics.Raycast(shooter.position, vectorToT, out hitLOS, Mathf.Infinity, layerMaskLOSCheck)) //ett layar som ignorerar allt förutom units o terräng
            {
                if (hitLOS.collider.transform == t)
                {
                    return true;
                }
            }
        }
        else //target är friendly -> då får jag använda ett annat layer så jag hittar denne
        {
            if (Physics.Raycast(shooter.position, vectorToT, out hitLOS, Mathf.Infinity, layerMaskLOSCheckFriendlyExcluded)) //nu kommer friendlys oxå kunna blocka denne, tänk på det
            {
                if (hitLOS.collider.transform == t)
                {
                    return true;
                }
            }
        }

        return false;
    }

}
