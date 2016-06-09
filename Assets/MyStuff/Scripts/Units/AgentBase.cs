using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(AgentStats))]
public class AgentBase : MonoBehaviour {
    enum UnitState { AttackMoving, Moving, Guarding};
    private UnitState state = UnitState.Guarding;

    private Transform thisTransform;
    private Health healthS;
    private AgentStats statsS;
    private NavMeshAgent agent;
    private Camera mainCamera;
    public Transform uiCanvas;
    public GameObject selectionMarkerObject;

    public string[] friendlyLayers;
    public string[] enemyLayers;

    private LayerMask friendlyOnly;
    private LayerMask enemyOnly;

    public float aggroDistance = 50;
    private List<Target> potTargets = new List<Target>(); //håller alla targets som kan vara, sen får man kolla vilka som kan nås och vilken aggro de har
    private Transform target;
    private float targetDistance; //så jag inte behöver räkna om denna på flera ställen

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
    private float attackSpeedTimer = 0.0f;

    public float startAttackRange = 4;
    [HideInInspector]
    public float attackRange;

    //stats****

    //variables used in different states:
    Transform[] tempTargets;

    private Vector3 startGuardPos;
    private Vector3 movePos; //för attackmove och move

	// Use this for initialization
	void Start () {
        thisTransform = this.transform;
        agent = thisTransform.GetComponent<NavMeshAgent>();
        healthS = thisTransform.GetComponent<Health>();
        statsS = thisTransform.GetComponent<AgentStats>();
        mainCamera = Camera.main;

        ToggleSelMarker(false);

        InitializeStats();
        InitializeLayerMask();

        Guard();
    }

    public virtual void InitializeStats()
    {
        damageMIN = startDamageMIN;
        damageMAX = startDamageMAX;

        attackSpeed = startAttackSpeed;

        attackRange = startAttackRange;
    }

    void InitializeLayerMask()
    {
        friendlyOnly |= thisTransform.gameObject.layer;
        for(int i = 0; i < friendlyLayers.Length; i++)
        {
            friendlyOnly |= (1 << LayerMask.NameToLayer(friendlyLayers[i]));
        }


        for (int i = 0; i < enemyLayers.Length; i++)
        {
            bool isValid = true;
            for(int y = 0; y < friendlyLayers.Length; y++) //kolla så att den inte är en friendly oxå
            {
                if (LayerMask.NameToLayer(enemyLayers[i]) == LayerMask.NameToLayer(friendlyLayers[y]))
                {
                    isValid = false;
                }
            }

            if (LayerMask.NameToLayer(enemyLayers[i]) == thisTransform.gameObject.layer)
            {
                isValid = false;
            }

            if (isValid == true)
            {
                enemyOnly |= (1 << LayerMask.NameToLayer(enemyLayers[i]));
            }
        }

    }
	
	// Update is called once per frame
	void Update () {
        uiCanvas.LookAt(uiCanvas.position + mainCamera.transform.rotation * Vector3.forward,
   mainCamera.transform.rotation * Vector3.up);

        if (target != null)
        {
            targetDistance = GetTargetDistance();
        }

        switch (state)
        {
            case UnitState.Guarding:
                tempTargets = ScanEnemies(aggroDistance);
                if(tempTargets != null && tempTargets.Length != 0)
                {
                    EngageTarget(ClosestTransform(tempTargets));
                }
                else
                {
                    if(GetStartGuardPointDistance() > 1.5f) //så att den inte ska jucka
                    {
                        agent.SetDestination(startGuardPos);
                    }
                }

                if(target != null && GetStartGuardPointDistance() < aggroDistance * 1.5f)
                {
                    AttackTarget();
                }
                else
                {
                    target = null;
                    agent.SetDestination(startGuardPos);
                }
                break;


            case UnitState.AttackMoving:
                tempTargets = ScanEnemies(aggroDistance);
                if (tempTargets != null && tempTargets.Length != 0)
                {
                    EngageTarget(ClosestTransform(tempTargets));                    
                }
                else //inga targets, återvänd till pathen typ
                {
                    agent.SetDestination(movePos);
                }

                //den borde tröttna på att jaga efter en viss stund
                break;

            case UnitState.Moving: //nått som kollar ifall jag kommit fram och isåfall vill jag nog vakta
                if(GetMovePosDistance() < 1.5f)
                {
                    Guard();
                }
                break;
        }
	}

    public virtual void AttackTarget()
    {
        if (attackRange > targetDistance) //kolla så att target står framför mig oxå
        {
            if (attackSpeedTimer <= Time.time)
            {
                attackSpeedTimer = attackSpeed + Time.time;
                int damageRoll = Random.Range(damageMIN, damageMAX);
                target.GetComponent<Health>().AddHealth(-damageRoll);
                //damage target!
            }
        }
        if (attackRange + 1  < targetDistance) //+1 för marginal
        {
            agent.SetDestination(target.position);
        }
    }

    public virtual void EngageTarget(Transform t)
    {
        target = t;
        targetDistance = GetTargetDistance();
    }

    public virtual void AttackMove(Vector3 pos)
    {
        state = UnitState.AttackMoving;
        movePos = pos;
    }

    public virtual void Move(Vector3 pos) //dålig prioritet när man rör på sig så man inte knuffar bort någon! tvärtom på stillastående
    {
        state = UnitState.Moving;
        agent.avoidancePriority = 50;
        movePos = pos;
        agent.SetDestination(movePos);
    }

    public virtual void Guard()
    {
        startGuardPos = thisTransform.position;
        state = UnitState.Guarding;
    }

    public virtual Transform[] ScanEnemies(float aD)
    {
        Collider[] hitColliders = Physics.OverlapSphere(thisTransform.position, aD, enemyOnly);
        //int i = 0;
        //while (i < hitColliders.Length)
        //{
        //    Debug.Log(hitColliders[i].transform.name);
        //    i++;
        //}
        Transform[] hits = new Transform[hitColliders.Length];
        for(int i = 0; i < hitColliders.Length; i++)
        {
            hits[i] = hitColliders[i].transform;
        }
        return hits;
    }
    public virtual bool TargetReachable(Transform target) //kolla ifall jag kan nå transformen
    {

        return true;
    }
    public virtual Transform ClosestTransform(Transform[] transforms)
    {
        Transform closest = transforms[0];
        for(int i = 0; i < transforms.Length; i++)
        {
            if(Vector3.Distance(thisTransform.position, closest.position) > Vector3.Distance(thisTransform.position, transforms[i].position))
            {
                closest = transforms[i];
            }
        }
        return closest;
    }

    public virtual Transform[] ScanFriendlys(float aD)
    {
        Collider[] hitColliders = Physics.OverlapSphere(thisTransform.position, aD, friendlyOnly);
        //int i = 0;
        //while (i < hitColliders.Length)
        //{
        //    Debug.Log(hitColliders[i].transform.name);
        //    i++;
        //}
        Transform[] hits = new Transform[hitColliders.Length];
        for (int i = 0; i < hitColliders.Length; i++)
        {
            hits[i] = hitColliders[i].transform;
        }
        return hits;
    }


    public virtual float GetTargetDistance()
    {
        return Vector3.Distance(thisTransform.position, target.position);
    }
    public float GetStartGuardPointDistance()
    {
        return Vector3.Distance(thisTransform.position, startGuardPos);
    }
    public float GetMovePosDistance()
    {
        return Vector3.Distance(thisTransform.position, movePos);
    }

    public void ToggleSelMarker(bool b)
    {
        selectionMarkerObject.SetActive(b);
    }
}

public struct Target
{
    Transform targetT;
    int aggro;

    public Target(Transform t, int a)
    {
        targetT = t;
        aggro = a;
    }
}
