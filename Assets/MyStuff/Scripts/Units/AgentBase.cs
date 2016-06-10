using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(AgentStats))]
public class AgentBase : MonoBehaviour {
    public enum UnitState { AttackMoving, Moving, Guarding};
    [HideInInspector]
    public UnitState state = UnitState.Guarding;

    [HideInInspector]public Transform thisTransform;
    [HideInInspector]public Health healthS;
    [HideInInspector]public AgentStats statsS;
    [HideInInspector]public NavMeshAgent agent;
    [HideInInspector]public Camera mainCamera;
    public Transform uiCanvas;
    public GameObject selectionMarkerObject;

    public string[] friendlyLayers; //bra att dessa är strings, behövs till tex AgentRanged
    public string[] enemyLayers;

    [HideInInspector]public LayerMask friendlyOnly;
    [HideInInspector]public LayerMask enemyOnly;

    public float aggroDistance = 20;
    [HideInInspector]
    public float temporaryAggroDistance = 0; //används när nått target skjuter långt
    //[HideInInspector]public List<Target> potTargets = new List<Target>(); //håller alla targets som kan vara, sen får man kolla vilka som kan nås och vilken aggro de har
    [HideInInspector]public Transform target;
    [HideInInspector]public AgentBase targetAgentBase;
    [HideInInspector]public float targetDistance; //så jag inte behöver räkna om denna på flera ställen

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

    public float startAttackRange = 4;
    [HideInInspector]
    public float attackRange;

    //stats****

    //variables used in different states:
    Transform[] tempTargets;

    [HideInInspector]public Vector3 startGuardPos;
    [HideInInspector]public Vector3 movePos; //för attackmove och move

	// Use this for initialization
	void Start () { //får jag dubbla starts?
        Init();
    }

    public virtual void Init()
    {
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

    public virtual void InitializeStats() //ha med andra påverkande faktorer här sedan
    {
        damageMIN = startDamageMIN;
        damageMAX = startDamageMAX;

        attackSpeed = startAttackSpeed;

        attackRange = startAttackRange;
    }

    void InitializeLayerMask()
    {
        friendlyOnly |= (1 << thisTransform.gameObject.layer);
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

            if (LayerMask.NameToLayer(enemyLayers[i]) == thisTransform.gameObject.layer) //kolla så att det inte är mitt eget layer
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
                GuardingUpdate();
                break;


            case UnitState.AttackMoving:
                AttackMovingUpdate();
                break;

            case UnitState.Moving: //nått som kollar ifall jag kommit fram och isåfall vill jag nog vakta
                MovingUpdate();
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
                if (targetAgentBase != null)
                {
                    targetAgentBase.Attacked(thisTransform); //notera att jag attackerat honom!
                }
            }
        }
        if (attackRange + 1  < targetDistance) //+1 för marginal
        {
            agent.SetDestination(target.position);
        }
    }

    public virtual void Attacked(Transform attacker) //blev attackerad av någon
    {
        bool validTarget = true;
        for(int i = 0; i < friendlyLayers.Length; i++) //kolla så att man inte råkas göra en friendly till target
        {
            if(LayerMask.LayerToName(attacker.gameObject.layer) == friendlyLayers[i])
            {
                validTarget = false;
                break;
            }
        }

        if (attacker.gameObject.layer == thisTransform.gameObject.layer)
        {
            validTarget = false;
        }


        if (validTarget == true)
        {
            if (target == null)
            {
                NewTarget(attacker);
                temporaryAggroDistance = GetDistanceToTransform(target);
            }
            else
            {
                Transform temp = ClosestTransform(target, attacker); //ta den som är närmst
                NewTarget(temp);

                if(target == attacker)
                {
                    temporaryAggroDistance = GetDistanceToTransform(target);
                }
            }
        }
    }

    public virtual void NewTarget(Transform t)
    {
        //kan man store:a den nya targetets stats som kan användas för att tex påverka skadan man gör på den
        NotifyNearbyFriendly(t.position); //viktigt denna kallas innan ett target sätts
        target = t;
        if(target.GetComponent<AgentBase>() != null)
        {
            targetAgentBase = target.GetComponent<AgentBase>();
        }
        else
        {
            targetAgentBase = null;
        }

        targetDistance = GetTargetDistance();
    }

    public virtual void AttackMove(Vector3 pos)
    {
        state = UnitState.AttackMoving;
        agent.avoidancePriority = 50;
        movePos = pos;
        agent.SetDestination(movePos);
    }

    public virtual void Move(Vector3 pos) //dålig prioritet när man rör på sig så man inte knuffar bort någon! tvärtom på stillastående
    {
        state = UnitState.Moving;
        agent.avoidancePriority = 50;
        movePos = pos;
        agent.SetDestination(movePos);
        target = null;
    }

    public virtual void Guard()
    {
        startGuardPos = thisTransform.position;
        state = UnitState.Guarding;
    }


    public virtual void GuardingUpdate()
    {
        if (target == null)
        {
            tempTargets = ScanEnemies(aggroDistance);
            if (tempTargets != null && tempTargets.Length != 0)
            {
                NewTarget(ClosestTransform(tempTargets));
            }
            else
            {
                if (GetStartGuardPointDistance() > 1.5f) //så att den inte ska jucka
                {
                    agent.SetDestination(startGuardPos);

                }
            }
        }

        if (target != null && GetStartGuardPointDistance() < aggroDistance * 1.5f + temporaryAggroDistance)
        {
            AttackTarget();
        }
        else
        {
            target = null;
            Move(startGuardPos);
            //agent.SetDestination(startGuardPos);
        }
    }

    public virtual void AttackMovingUpdate()
    {
        if (target == null)
        {
            tempTargets = ScanEnemies(aggroDistance);
            if (tempTargets != null && tempTargets.Length != 0)
            {
                NewTarget(ClosestTransform(tempTargets));
            }
            else //inga targets, återvänd till pathen typ
            {
                agent.SetDestination(movePos);
            }
        }


        if (target != null && GetTargetDistance() < aggroDistance * 1.05f + temporaryAggroDistance)
        {
            AttackTarget();
        }
        else
        {
            target = null;
            agent.SetDestination(movePos);
        }
        //den borde tröttna på att jaga efter en viss stund also

        if (GetMovePosDistance() < 1.5f)
        {
            Guard();
        }
    }

    public virtual void MovingUpdate()
    {
        if (GetMovePosDistance() < 1.5f)
        {
            Guard();
        }
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
    public virtual Transform ClosestTransform(Transform t1, Transform t2)
    {
        if(GetDistanceToTransform(t1) < GetDistanceToTransform(t2))
        {
            return t1;
        }
        else
        {
            return t2;
        }
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
            if (hits[i] != thisTransform) //vill väl inte ha mig själv i listan?
            {
                hits[i] = hitColliders[i].transform;
            }
        }
        return hits;
    }
    public virtual void NotifyNearbyFriendly(Vector3 pos)
    {
        if (target == null)
        {
            if (state == UnitState.Guarding) //inte attackmoving, då blire endless loop
            {
                Transform[] nearbyFriendly = ScanFriendlys(aggroDistance*20);

                for (int i = 0; i < nearbyFriendly.Length; i++)
                {
                    if (nearbyFriendly[i].gameObject.activeSelf == true)
                    {
                        nearbyFriendly[i].GetComponent<AgentBase>().AttackMove(pos); //behöver något annat än attackmove, investigate
                    }
                }

                AttackMove(pos); //här kan behövas en annan, en som typ flyttar tillbaks dem till samma ställe efter, investigate elr nått
            }
        }
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
    public float GetDistanceToTransform(Transform t)
    {
        return Vector3.Distance(thisTransform.position, t.position);
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
