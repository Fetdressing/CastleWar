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
    // Use this for initialization
    void Start () {
        Init();
	}
	
	// Update is called once per frame
	void Update () {
        //Debug.Log(startPos.ToString());
        uiCanvas.LookAt(uiCanvas.position + mainCamera.transform.rotation * Vector3.forward,
   mainCamera.transform.rotation * Vector3.up);

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
            layerMaskLOSCheck ^= (1 << LayerMask.NameToLayer(friendlyLayers[i]));
        }

        //layerMaskLOSCheck |= (1 << friendlyOnly); //lägg till sin egen layer så att man kan skjuta igenom allierade
    }

    public override void AttackTarget()
    {
        bool los;
        los = LineOfSight(target);
        if (attackRange > targetDistance) //kolla så att target står framför mig oxå
        {
            if (attackSpeedTimer <= Time.time)
            {
                attackSpeedTimer = attackSpeed + Time.time;
                int damageRoll = Random.Range(damageMIN, damageMAX);
                
                if(los)
                {
                    Fire(damageRoll);
                }
            }
        }
        if (attackRange + 1 < targetDistance || !los) //+1 för marginal
        {
            agent.SetDestination(target.position);
        }
        else if(targetDistance < minimumTargetDistance)
        {
            Vector3 vectorFromTarget = thisTransform.position - target.position;
            agent.SetDestination(thisTransform.position + vectorFromTarget);
        }
        else
        {
            agent.ResetPath();
        } 
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

            if (targetAgentBase != null)
            {
                lastProjectileScript.Fire(target, damageRoll, 4, true);
            }
            else //bara typ nån destructable
            {
                lastProjectileScript.Fire(target, damageRoll, 4, false);
            }
            
        }
    }

    public bool LineOfSight(Transform t) //has LOS to t?
    {
        RaycastHit hitLOS;
        Vector3 vectorToT = t.position - thisTransform.position;
        if (Physics.Raycast(thisTransform.position, vectorToT, out hitLOS, Mathf.Infinity, layerMaskLOSCheck)) //en layermask som ignorerar allt förutom terräng
        {
            if(hitLOS.collider.transform == t)
            {
                return true;
            }
        }

        return false;
    }

}
