using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tower : BuildingBase {
    [Header("Ranged")]
    public GameObject projectile;
    [HideInInspector]
    public List<GameObject> projectilePool = new List<GameObject>();
    public int projectilePoolSize = 15;
    public Transform shooter;

    //stats****
    [Header("Stats")]
    public int startDamageMIN = 3;
    public int startDamageMAX = 6;
    [HideInInspector]
    public int damageMIN;
    [HideInInspector]
    public int damageMAX;

    public float startAttackSpeed = 1.2f;
    [HideInInspector]
    public float attackSpeed; //public så att agentStats kan påverka den
    [HideInInspector]
    public float attackSpeedTimer = 0.0f;

    public float startAttackRange = 30;
    [HideInInspector]
    public float attackRange;

    public float minimumTargetDistance = 5;
    [HideInInspector]
    public float targetDistance;
    //stats****

    public LayerMask layerMaskLOSCheck;
    [HideInInspector]
    public LayerMask layerMaskLOSCheckFriendlyExcluded; //samma som layerMaskLOSCheck fast MED sin egen layer

    [HideInInspector]
    public List<Transform> targetList = new List<Transform>();

    [HideInInspector]
    public Transform target;
    [HideInInspector]
    public Health targetHealth;
    [HideInInspector]
    public AIBase targetBase;
    [HideInInspector]
    public bool isFriendlyTarget;

    public override void Init()
    {
        base.Init();
        CreateObjectPool();

        for (int i = 0; i < friendlyLayers.Count; i++)
        {
            layerMaskLOSCheck |= (1 << LayerMask.NameToLayer(friendlyLayers[i])); //lägg till friendly layers
        }
        for (int i = 0; i < enemyLayers.Length; i++)
        {
            layerMaskLOSCheckFriendlyExcluded |= (1 << LayerMask.NameToLayer(enemyLayers[i]));
            layerMaskLOSCheck |= (1 << LayerMask.NameToLayer(enemyLayers[i])); //lägg till friendly layers
        }
        layerMaskLOSCheck |= (1 << LayerMask.NameToLayer("Terrain"));
        layerMaskLOSCheckFriendlyExcluded |= (1 << LayerMask.NameToLayer("Terrain"));

        InitializeStats();
        Reset();
    }

    public virtual void InitializeStats() //ha med andra påverkande faktorer här sedan
    {
        damageMIN = startDamageMIN;
        damageMAX = startDamageMAX;

        attackSpeed = startAttackSpeed;

        attackRange = startAttackRange;
    }

    void CreateObjectPool()
    {
        for (int i = 0; i < projectilePoolSize; i++)
        {
            GameObject tempO = Instantiate(projectile.gameObject) as GameObject;
            tempO.GetComponent<Projectile>().Init(enemyLayers, friendlyLayers, thisTransform);
            projectilePool.Add(tempO.gameObject);
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

    public override void Dealloc()
    {
        base.Dealloc();
        for (int i = 0; i < projectilePool.Count; i++)
        {
            projectilePool[i].GetComponent<Projectile>().Dealloc();
            Destroy(projectilePool[i].gameObject);
        }
    }

    void Start()
    {
        //Init();
    }

    void Awake()
    {
        Init();
    }

    void Update() //kan använda en corutine istället med attackspeeden som yield
    {
        if(target != null)
        {
            targetDistance = GetDistanceToTransform(target);
            if (AttackTarget() == false)
            {
                //reset target för det går inte ha kvar det targetet
                target = null;
            }
        }
        else
        {
            if(!ExecuteNextCommand()) //hitta ett target för det fanns inga kommandon i kö
            {
                SearchTarget();
            }
        }
    }


    public void NewTarget(Transform t)
    {
        target = t;
        targetHealth = target.GetComponent<Health>();
        if (target.GetComponent<AIBase>() != null)
        {
            targetBase = target.GetComponent<AIBase>();
        }
        targetDistance = GetDistanceToTransform(target);

        isFriendlyTarget = IsFriendly(t);
    }

    public virtual bool AttackTarget()
    {
        bool los;
        los = LineOfSight();
        float targetDistanceuS = (targetDistance - targetHealth.unitSize);

        bool targetValid = true;
        if (target == null || target.gameObject.activeSelf == false || !targetHealth.IsAlive() || targetDistanceuS > attackRange || targetDistanceuS < minimumTargetDistance || !los)
        {
            targetValid = false;
            return targetValid;
        }

        if (attackRange > targetDistanceuS && targetDistanceuS > minimumTargetDistance) //kolla så att target står framför mig oxå
        {
            if (attackSpeedTimer <= Time.time)
            {
                if (los)
                {
                    attackSpeedTimer = attackSpeed + Time.time;
                    int damageRoll = Random.Range(damageMIN, damageMAX);
                    Fire(damageRoll); //den skjuter på marken, på fiende transformen, detta skulle kunna vara mer reliable
                }
            }
        }
        return true;
    }

    public virtual void Fire(int damageRoll)
    {
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

    public virtual void SearchTarget()
    {
        Transform[] potTargets = ScanEnemies(attackRange);
        if (potTargets == null)
        {
            return;
        }

        for(int i = 0; i < potTargets.Length; i++)
        {
            float tDistance = GetDistanceToTransform(potTargets[i]);
            if(tDistance < attackRange && tDistance > minimumTargetDistance)
            {
                bool los = LineOfSight(potTargets[i]);
                if(los)
                {
                    NewTarget(potTargets[i]);
                    return;
                }
            }
        }
    }


    public bool LineOfSight()
    {
        RaycastHit hitLOS;
        Vector3 vectorToT = targetHealth.middlePoint - shooter.position; //hämta mittpunkten istället
        //hade behövt ignorera sig själv
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

    public bool LineOfSight(Transform t)
    {
        RaycastHit hitLOS;
        Vector3 vectorToT = t.GetComponent<Health>().middlePoint - shooter.position; //hämta mittpunkten istället

        if (Physics.Raycast(shooter.position, vectorToT, out hitLOS, Mathf.Infinity, layerMaskLOSCheck)) //ett layar som ignorerar allt förutom units o terräng
        {
            if (hitLOS.collider.gameObject.layer != LayerMask.NameToLayer("Terrain"))
            {
                return true;
            }
        }
        return false;
    }

    public override void AttackUnit(Transform t, bool friendlyFire)
    {
        if (t != null && t.gameObject.activeSelf == true && t.GetComponent<Health>().IsAlive())
        {
            NewTarget(t);
        }
    }

    public override void AddCommandToList(Vector3 pos, UnitState nextState, Transform tar, bool friendlyfire) //den ska ju bara kunna attackera units så denna behöver moddas
    {
        if(tar != null) //betyder att det är "attackunit"
        {
            targetList.Add(tar);
        }

        //Command c = new Command(nextState, pos, tar, friendlyfire);
        //if (nextCommando.Count > 5) //vill inte göra denna lista hur lång som helst
        //{
        //    nextCommando[nextCommando.Count - 1] = c; //släng på den på sista platsen
        //    return;
        //}
        //nextCommando.Add(c);
    }

    public new bool ExecuteNextCommand() //returnerar ifall det fanns ett kommando att utföra
    {
        //base.ExecuteNextCommand();
        if(targetList.Count > 0)
        {
            NewTarget(targetList[0]);
            targetList.RemoveAt(0);
            return true;
        }
        return false;
    }

    public override void ClearCommands()
    {
        //base.ClearCommands();
        targetList.Clear();
    }
}
