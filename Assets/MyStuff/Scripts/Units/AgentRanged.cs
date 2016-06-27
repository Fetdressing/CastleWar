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

    public LayerMask layerMaskLOSCheck;
    [HideInInspector]
    public LayerMask layerMaskLOSCheckFriendlyIncluded; //samma som layerMaskLOSCheck fast MED sin egen layer

    // Use this for initialization
 //   void Start () {
 //       Init();
	//}

    void Awake()
    {
        Init();
    }
	
	// Update is called once per frame
	void Update () {
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

    public virtual void Init()
    {
        base.Init();

        for(int i = 0; i < projectilePoolSize; i++)
        {
            GameObject tempO = Instantiate(projectile.gameObject) as GameObject;
            tempO.GetComponent<Projectile>().Init(enemyLayers, friendlyLayers, thisTransform);
            projectilePool.Add(tempO.gameObject);
        }


        for (int i = 0; i < friendlyLayers.Count; i++)
        {
            layerMaskLOSCheck |= (1 << LayerMask.NameToLayer(friendlyLayers[i])); //ta bort friendly layers
        }
        for (int i = 0; i < enemyLayers.Length; i++)
        {
            layerMaskLOSCheck |= (1 << LayerMask.NameToLayer(enemyLayers[i])); //ta bort friendly layers
        }
        layerMaskLOSCheck |= (1 << LayerMask.NameToLayer("Terrain"));

        //layerMaskLOSCheckFriendlyIncluded = layerMaskLOSCheck; //set denna innan så att den får med alla friendly layers
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
                    int damageRoll = Random.Range(damageMIN, damageMAX);
                    Fire(damageRoll); //den skjuter på marken, på fiende transformen, detta skulle kunna vara mer reliable
                }
            }
        }

        if (attackRange + 1 < targetDistanceuS || !los) //+1 för marginal
        {
            agent.SetDestination(target.position);
        }
        else if(targetDistanceuS <= minimumTargetDistance)
        {
            Vector3 vectorFromTarget = thisTransform.position - target.position;
            agent.SetDestination(thisTransform.position + vectorFromTarget); //gånger ett värde för att förflytta denne lite extra
        }
        else if (!isFacingTarget) //den resetar pathen för ovanstående när den går utanför minimumTargetDistance
        {
            RotateTowards(target);
            agent.ResetPath();
        }
        else
        {
            agent.ResetPath();
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
                    lastProjectileScript.Fire(target, targetHealth.middlePoint, damageRoll, 4, true, true);
                }
                else
                {
                    lastProjectileScript.Fire(target, targetHealth.middlePoint, damageRoll, 4, true, false); //annars inte
                }
            }
            else //bara typ nån destructable
            {
                lastProjectileScript.Fire(target, targetHealth.middlePoint, damageRoll, 4, false, false);
            }
            
        }
    }

    public bool LineOfSight() //has LOS to t?
    {
        RaycastHit hitLOS;
        //RaycastHit[] hitsLOS;
        Vector3 vectorToT = targetHealth.middlePoint - thisTransform.position; //hämta mittpunkten istället

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

        if (Physics.Raycast(thisTransform.position, vectorToT, out hitLOS, Mathf.Infinity, layerMaskLOSCheck)) //ett layar som ignorerar allt förutom units o terräng
        {
            if (hitLOS.collider.gameObject.layer != LayerMask.NameToLayer("Terrain"))
            {
                //if (hitLOS.transform == target) //vet inte ifall jag vill ha med denna checken eller ej, förmodligen så
                //{
                return true;
                //}
            }
        }
        return false;
        //if (!IsFriendly(target))
        //{
        //    if (Physics.Raycast(thisTransform.position, vectorToT, out hitLOS, Mathf.Infinity, layerMaskLOSCheck)) //ett layar som ignorerar allt förutom units o terräng
        //    {
        //        if (hitLOS.collider.transform == target)
        //        {
        //            return true;
        //        }
        //    }
        //}
        //else //target är friendly -> då får jag använda ett annat layer så jag hittar denne
        //{
        //    if (Physics.Raycast(thisTransform.position, vectorToT, out hitLOS, Mathf.Infinity, layerMaskLOSCheckFriendlyIncluded)) //nu kommer friendlys oxå kunna blocka denne, tänk på det
        //    {
        //        if (hitLOS.collider.transform == target)
        //        {
        //            return true;
        //        }
        //    }
        //}

        //return false;
    }
    public bool LineOfSight(Transform t) //has LOS to t?
    {
        RaycastHit hitLOS;
        Vector3 vectorToT = t.GetComponent<Health>().middlePoint - thisTransform.position; //hämta mittpunkten istället

        if (!IsFriendly(target))
        {
            if (Physics.Raycast(thisTransform.position, vectorToT, out hitLOS, Mathf.Infinity, layerMaskLOSCheck)) //ett layar som ignorerar allt förutom units o terräng
            {
                if (hitLOS.collider.transform == t)
                {
                    return true;
                }
            }
        }
        else //target är friendly -> då får jag använda ett annat layer så jag hittar denne
        {
            if (Physics.Raycast(thisTransform.position, vectorToT, out hitLOS, Mathf.Infinity, layerMaskLOSCheckFriendlyIncluded)) //nu kommer friendlys oxå kunna blocka denne, tänk på det
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
