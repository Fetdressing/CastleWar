using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(AgentStats))]
public class AgentBase : AIBase {
    [HideInInspector]
    public UnitState state;

    [HideInInspector]public Health healthS;
    [HideInInspector]public AgentStats statsS;
    [HideInInspector]public NavMeshAgent agent;

    public float aggroDistance = 20;
    [HideInInspector]
    public float temporaryAggroDistance = 0; //används när nått target skjuter långt
    //[HideInInspector]public List<Target> potTargets = new List<Target>(); //håller alla targets som kan vara, sen får man kolla vilka som kan nås och vilken aggro de har
    [HideInInspector]public Transform target;
    [HideInInspector]public AIBase targetBase;
    [HideInInspector]public Health targetHealth;
    [HideInInspector]public float targetDistance; //så jag inte behöver räkna om denna på flera ställen
    [HideInInspector]public bool isFriendlyTarget;

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

    [Header("Other")]
    //variables used in different states*****

    //almost reach target**
    public float reachDistanceThreshhold = 10; //hur nära som är 'close enough'
    bool closeToEnd = false;
    float timerReachEnd; //när denna nästan nått pathen så set en timer för att inte stå o jucka för all evighet, finns risk att denne inte kan nå target
    float reachEndPatience = 2.0f; //om den är hyfsat länge till slutpunkten så hur länge den ska vänta innan den ger upp och nöjer sig med nuvarande punkt    
    //almost reach target**

    //look angle threshhold, hur mycket den måste titta på fienden för att kunna attackera
    private float lookAngleThreshhold = 25;
    public float turnRatio = 2;

    public LayerMask layerMaskLOSCheck;
    [HideInInspector]
    public LayerMask layerMaskLOSCheckFriendlyExcluded; //samma som layerMaskLOSCheck fast MED sin egen layer

    [HideInInspector]
    public float startChaseTime;
    [HideInInspector]
    public float chaseTimeAggressive = 6;
    [HideInInspector]
    public float chaseTimeNormal = 3;

    Transform[] tempTargets; //används för att skanna nearby fiender tex

    [HideInInspector]public Vector3 startPos;
    [HideInInspector]public Vector3 startPos2; //används när man tex frångår pathen
    [HideInInspector]public Vector3 movePos; //för attackmove och move


    bool ignoreSurrounding = false; //används för att återta en path så att denne inte försöker jaga fiender hela tiden

    //variables used in different states*****

    // Use this for initialization
    void Start()
    { //får jag dubbla starts?
        Init();
    }

    void Awake()
    {
        //Init();
    }

    public override void Reset()
    {
        closeToEnd = false;
        ignoreSurrounding = false;
        Guard();
    }

    public override void Init()
    {
        base.Init();
        agent = thisTransform.GetComponent<NavMeshAgent>();
        healthS = thisTransform.GetComponent<Health>();
        statsS = thisTransform.GetComponent<AgentStats>();

        InitializeStats();

        //Reset(); inte här
    }

    public virtual void InitializeStats() //ha med andra påverkande faktorer här sedan
    {
        damageMIN = startDamageMIN;
        damageMAX = startDamageMAX;

        attackSpeed = startAttackSpeed;

        attackRange = startAttackRange;
    }
	
	// Update is called once per frame
	void Update () {
        if (healthS.IsAlive() && thisTransform.gameObject.activeSelf == true)
        {
            if (target != null)
            {
                targetDistance = GetTargetDistance();
            }
            //Debug.Log(state.ToString());
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
	}

    public virtual bool AttackTarget()
    {
        if (!IsActive())
            return false;

        bool targetAlive = true;
        if(target == null || target.gameObject.activeSelf == false || !targetHealth.IsAlive())
        {
            targetAlive = false;
            return targetAlive;
        }

        bool isFacingTarget = IsFacingTransform(target);

        if (attackRange > (targetDistance-targetHealth.unitSize) && isFacingTarget) //kolla så att target står framför mig oxå
        {
            if (attackSpeedTimer <= Time.time)
            {
                attackSpeedTimer = attackSpeed + Time.time;
                int damageRoll = Random.Range(damageMIN, damageMAX);

                if (targetHealth.AddHealth(-damageRoll)) //target överlevde attacken
                {
                    targetAlive = true;
                }
                else //target dog
                {
                    targetAlive = false;
                }

                if (targetBase != null)
                {
                    targetBase.Attacked(thisTransform); //notera att jag attackerat honom!
                }
            }
        }
        if (attackRange * 0.7f < (targetDistance-targetHealth.unitSize)) //marginal med jue
        {
            SetDestination(target.position);
        }
        else if(!isFacingTarget)
        {
            RotateTowards(target);
            ResetPath();
        }
        else
        {
            ResetPath();
        }

        return targetAlive;
    }

    public override void Attacked(Transform attacker) //blev attackerad av någon
    {
        if (state != UnitState.AttackingUnit && state != UnitState.Moving && attacker != null)
        {
            bool validTarget = true;
            for (int i = 0; i < friendlyLayers.Count; i++) //kolla så att man inte råkas göra en friendly till target
            {
                if (LayerMask.LayerToName(attacker.gameObject.layer) == friendlyLayers[i])
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
                }
                else
                {
                    Transform temp = ClosestTransform(target, attacker); //ta den som är närmst
                    NewTarget(temp);
                }
            }
        }
    }

    public virtual void NewTarget(Transform t)
    {
        //kan man store:a den nya targetets stats som kan användas för att tex påverka skadan man gör på den
        NotifyNearbyFriendly(t.position); //viktigt denna kallas innan ett target sätts
        startChaseTime = Time.time;
        target = t;
        targetHealth = target.GetComponent<Health>();
        if (target.GetComponent<AIBase>() != null)
        {
            targetBase = target.GetComponent<AIBase>();
        }
        else
        {
            targetBase = null;
        }

        targetDistance = GetTargetDistance();
        isFriendlyTarget = IsFriendly(target);
    }

    public virtual void SetTarget(Transform t) //när man order attack, så att den inte allertar allierade
    {
        startChaseTime = Time.time;
        target = t;
        targetHealth = target.GetComponent<Health>();
        if (target.GetComponent<AIBase>() != null)
        {
            targetBase = target.GetComponent<AIBase>();
        }
        else
        {
            targetBase = null;
        }

        targetDistance = GetTargetDistance();
        isFriendlyTarget = IsFriendly(target);
    }


    public override void AttackMove(Vector3 pos)
    {
        if (!IsActive())
            return;
        ResetPath();
        state = UnitState.AttackMoving;
        agent.avoidancePriority = 1;
        movePos = pos;
        SetDestination(movePos);
        ignoreSurrounding = false;
        closeToEnd = false;
    }

    public override void Move(Vector3 pos) //dålig prioritet när man rör på sig så man inte knuffar bort någon! tvärtom på stillastående
    {
        if (!IsActive())
            return;
        ResetPath();
        state = UnitState.Moving;
        agent.avoidancePriority = 1;
        movePos = pos;
        SetDestination(movePos);
        target = null;
        closeToEnd = false;
    }

    public override void Guard()
    {
        if (!IsActive())
            return;
        target = null;
        ResetPath();
        nextCommando.Clear(); //för man kan ju inte ha Guard i en kedja duh
        agent.avoidancePriority = 100;
        startPos = thisTransform.position;
        state = UnitState.Guarding;
    }

    public override void Guard(Vector3 pos) //redefinition
    {
        if (!IsActive())
            return;
        ResetPath();
        nextCommando.Clear(); //för man kan ju inte ha Guard i en kedja duh
        agent.avoidancePriority = 100;
        startPos = pos;
        state = UnitState.Guarding;
    }

    public override void Investigate(Vector3 pos)
    {
        if (!IsActive())
            return;
        if (target == null)
        {
            if (state == UnitState.Guarding) //vill ju inte den tex ska påbörja en ny investigate
            {
                ResetPath();
                state = UnitState.Investigating;
                agent.avoidancePriority = 1;
                movePos = pos;
                startPos = thisTransform.position;
                ignoreSurrounding = false;
                closeToEnd = false; //vill kanske använda den här oxå?
                //SetDestination(movePos);
            }
        }
    }

    public override void AttackUnit(Transform t, bool friendlyFire)
    {
        if (t != null && t.gameObject.activeSelf == true)
        {
            if (!IsActive())
                return;
            if (t != thisTransform)
            {
                bool canAttack = false;
                if (IsFriendly(t) && friendlyFire)
                {
                    canAttack = true;
                }
                else if (!IsFriendly(t))
                {
                    canAttack = true;
                }

                if (t.GetComponent<Health>() == null)
                {
                    canAttack = false;
                }

                if (canAttack)
                {
                    ResetPath();
                    state = UnitState.AttackingUnit;
                    agent.avoidancePriority = 100;
                    SetDestination(t.position);
                    SetTarget(t); //set target så den inte alertar allierade i onödan
                }
                else
                {
                    AttackMove(t.position); //om det inte är valid target så bara gå dit istället 
                }
            }
            else
            {
                ExecuteNextCommand();
            }
        }
    }

    public override void ExecuteNextCommand()
    {
        if(nextCommando.Count <= 0)
        {
            nextCommando.Clear();
            //default kommando
            switch(state)
            {
                case UnitState.Moving:
                    Guard(); //får väl se ifall någon av dessa behöver ändras men känns rätt stabilt
                    break;
                case UnitState.AttackMoving:
                    Guard();
                    break;
                case UnitState.Investigating:
                    Guard();
                    break;
                case UnitState.Guarding:
                    Guard();
                    break;
                case UnitState.AttackingUnit:
                    Guard();
                    break;
            }
        }
        else //finns states som ska köras
        {
            //state = nextCommando[0].stateToExecute;
            Vector3 pos = nextCommando[0].positionToExecute;
            Transform t = nextCommando[0].target;
            bool friendfire = nextCommando[0].friendlyFire;
            switch (nextCommando[0].stateToExecute)
            {
                case UnitState.Moving:
                    nextCommando.RemoveAt(0); //viktigt denna körs innan själva kommandot så det ej blir en endless loop
                    Move(pos);
                    break;
                case UnitState.AttackMoving:
                    nextCommando.RemoveAt(0); //ta bort den kommandot som kördes igång :)
                    AttackMove(pos);
                    break;
                case UnitState.Investigating:
                    nextCommando.RemoveAt(0); //ta bort den kommandot som kördes igång :)
                    Investigate(pos);
                    break;
                case UnitState.Guarding:
                    nextCommando.RemoveAt(0); //ta bort den kommandot som kördes igång :)
                    Guard(pos);
                    break;
                case UnitState.AttackingUnit:
                    nextCommando.RemoveAt(0); //ta bort den kommandot som kördes igång :)
                    AttackUnit(t, friendfire);
                    break;
            }
        }

    }

    public override void AddCommandToList(Vector3 pos, UnitState nextState, Transform tar, bool friendlyfire)
    {
        Command c = new Command(nextState, pos, tar, friendlyfire);
        if(nextCommando.Count > 5) //vill inte göra denna lista hur lång som helst
        {
            nextCommando[nextCommando.Count-1] = c; //släng på den på sista platsen
            return;
        }
        nextCommando.Add(c);
    }


    public virtual void GuardingUpdate()
    {
        if (target == null)
        {
            ExecuteNextCommand();
            tempTargets = ScanEnemies(aggroDistance);
            if (tempTargets != null && tempTargets.Length != 0)
            {
                NewTarget(ClosestTransform(tempTargets));
            }
            else
            {
                if (GetStartPointDistance() > 1.5f) //så att den inte ska jucka
                {
                    SetDestination(startPos);
                }
            }
        }

        if (target != null)
        {
            if (GetTargetDistance() > attackRange * 1.5f) //check for closer target
            {
                Transform potTarget = CheckForBetterTarget(attackRange);
                if (potTarget != null)
                {
                    NewTarget(potTarget);
                }
            }

            if (GetStartPointDistance() < aggroDistance * 1.5f || (startChaseTime + chaseTimeNormal) > Time.time)
            {
                if (AttackTarget() == false) //target död?
                {
                    target = null;
                }
            }
            else
            {
                target = null;
                Move(startPos); //kanske borde ha något dynamiskt event här
            }
        }
    }

    public virtual void AttackMovingUpdate()
    {
        if (target == null && ignoreSurrounding == false)
        {
            tempTargets = ScanEnemies(aggroDistance);
            if (tempTargets != null && tempTargets.Length != 0)
            {
                NewTarget(ClosestTransform(tempTargets));
                startPos2 = thisTransform.position; //därifrån den börja jaga, så att den kan återupta sin path efter den jagat
            }
            else //inga targets, återvänd till pathen typ
            {
                SetDestination(movePos);
            }
        }


        if (target != null)
        {
            if(GetTargetDistance() > attackRange * 1.5f) //check for closer target
            {
                Transform potTarget = CheckForBetterTarget(attackRange);
                if(potTarget != null)
                {
                    NewTarget(potTarget);
                }
            }

            if (GetTargetDistance() < aggroDistance * 1.05f || (startChaseTime + chaseTimeNormal) > Time.time)
            {
                //Debug.Log("Attack");
                if(AttackTarget() == false)
                {
                    target = null;
                }
            }
            else
            {
                target = null;
                SetDestination(startPos2); //återvänd till pathen
                ignoreSurrounding = true;
            }
        }

        if (ignoreSurrounding && GetStartPointDistance2() < attackRange) //kommit tillbaks till pathen -> fortsätt attackmove!
        {
            ignoreSurrounding = false;
        }


        if (agent.pathPending) //så agent.remainingDistance funkar
        { }
        else if (GetMovePosDistance() < 1.5f || agent.remainingDistance < 1.5f) //kom fram
        {
            ExecuteNextCommand();
        }
        else if (target == null && IsCloseEnoughToPos(movePos)) //den får inte vara klar bara för att den råkar passera punkten när den jagar ett target
        {
            ExecuteNextCommand();
        }
    }

    public virtual void MovingUpdate()
    {
        if(agent.pathPending) //så agent.remainingDistance funkar
        { }
        else if (GetMovePosDistance() < 1.5f || IsCloseEnoughToPos(movePos) || agent.remainingDistance < 1.5f) //ifall man är klar, denna måste bli klar also efter tid eller liknande, stora units har svårt att nå fram. Kanske nått med grupp stuff o göra?
        {
            ExecuteNextCommand(); //ha ett storeat 'next command', finns inget så kör default ofc!
        }
    }

    public virtual void InvestigatingUpdate() //HÄR ÄR FELET!, se över denna, lite oklart när de ger upp chasen o återvänder hem
    {
        if (target == null && ignoreSurrounding == false)
        {
            tempTargets = ScanEnemies(aggroDistance);
            if (tempTargets != null && tempTargets.Length != 0)
            {
                NewTarget(ClosestTransform(tempTargets));
                startPos2 = thisTransform.position; //därifrån den börja jaga, så att den kan återupta sin investigation efter den jagat
                //AddCommandToList(startPos2, UnitState.Moving); //dessa blir mega stackade!!!!!!!!!!!!!!
                //AddCommandToList(movePos, UnitState.Investigating); //fortsätt där den slutade
            }
            else
            {
                if (GetMovePosDistance() > attackRange) //så att den inte ska jucka
                {
                    SetDestination(movePos);
                }
                else //återvänd hem igen
                {
                    //Debug.Log("GO home!");
                    Move(startPos);
                }
            }
        }

        if (target != null)
        {
            if (GetTargetDistance() > attackRange * 1.5f) //check for closer target
            {
                Transform potTarget = CheckForBetterTarget(attackRange);
                if (potTarget != null)
                {
                    NewTarget(potTarget);
                }
            }

            if (GetStartPointDistance2() < aggroDistance || (startChaseTime + chaseTimeNormal) > Time.time) //måste kunna återvända till där den var innan den påbörja pathen
            {
                if (AttackTarget() == false)
                {
                    target = null;
                }
            }
            else
            {
                target = null;
                SetDestination(startPos2);
                //Move(startPos2);
                ignoreSurrounding = true;
                //ExecuteNextCommand();
                //SetDestination(startPos);
            }
        }    
        if(ignoreSurrounding && IsCloseEnoughToPos(startPos2)) //kommit tillbaks till pathen -> fortsätt investigate!
        {
            ignoreSurrounding = false;
        }

        if (IsCloseEnoughToPos(movePos)) //så att den inte ska jucka, när jag nått target så dra hem igen!
        {
            //Debug.Log("GO home!");
            Move(startPos);
        }
    }

    public virtual void AttackUnitUpdate()
    {
        bool targetAlive = AttackTarget();

        if (target == null || targetAlive == false) //kom fram
        {
            ExecuteNextCommand();
        }
    }


    public virtual bool TargetReachable(Transform target) //kolla ifall jag kan nå transformen
    {

        return true;
    }
    public virtual Transform ClosestTransform(Transform[] transforms)
    {
        if (transforms != null)
        {
            Transform closest = transforms[0];
            for (int i = 0; i < transforms.Length; i++)
            {
                if (Vector3.Distance(thisTransform.position, closest.position) > Vector3.Distance(thisTransform.position, transforms[i].position))
                {
                    closest = transforms[i];
                }
            }
            return closest;
        }
        else return null;
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
    public virtual bool IsCloseEnoughToPos(Vector3 endPos) //använder tid för att inte försöka för evigt
    {
        //if (!agent.hasPath)
        //{
        //    Debug.Log("No path");
        //    return true;
        //}
        if (closeToEnd == false)
        {
            if (GetDistanceToPosition(endPos) < reachDistanceThreshhold) //jag är nära nog, påbörja count down!
            {
                closeToEnd = true;
                timerReachEnd = Time.time + reachEndPatience; //påbörja countdown
            }
            return false;
        }
        else if (closeToEnd == true)
        {
            if (timerReachEnd < Time.time)
            {
                closeToEnd = false;
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }
    public virtual Transform CheckForBetterTarget(float d)
    {
        Transform[] potTargets = ScanEnemies(attackRange);
        if(potTargets == null)
        {
            return null;
        }

        Transform tTemp = ClosestTransform(potTargets);

        if (GetTargetDistance() > GetDistanceToTransform(tTemp))
        {
            return tTemp;
        }
        return null;
    }
    

    public override void NotifyNearbyFriendly(Vector3 pos)
    {
        if (state == UnitState.Guarding) //inte attackmoving, då blire endless loop
        {
            List<Transform> nearbyFriendly = ScanFriendlys(aggroDistance);

            for (int i = 0; i < nearbyFriendly.Count; i++)
            {
                if (nearbyFriendly[i].gameObject.activeSelf == true && nearbyFriendly[i].GetComponent<AgentBase>() != null)
                {
                    nearbyFriendly[i].GetComponent<AgentBase>().Investigate(pos); //behöver något annat än attackmove, investigate
                }
            }
        }
    }

    public float GetTargetDistance()
    {
        if (target != null)
        {
            return Vector3.Distance(thisTransform.position, target.position); //ska kanske vara mot healthmidpoint
        }
        else return 0;
    }
    public float GetStartPointDistance()
    {
        return Vector3.Distance(thisTransform.position, startPos);
    }
    public float GetStartPointDistance2()
    {
        //Debug.Log(Vector3.Distance(thisTransform.position, startPos2).ToString());
        return Vector3.Distance(thisTransform.position, startPos2);
    }
    public float GetMovePosDistance()
    {
        return Vector3.Distance(thisTransform.position, movePos);
    }

    public bool IsFacingTransform(Transform t)
    {
        Vector3 tPosWithoutY = new Vector3(t.position.x, thisTransform.position.y, t.position.z); //så den bara kollar på x o z leden
        Vector3 vecToTransform = tPosWithoutY - thisTransform.position;
        float angle = Vector3.Angle(thisTransform.forward, vecToTransform);

        if(angle > lookAngleThreshhold)
        {
            //Debug.Log(angle.ToString());
            return false;
        }
        return true;
    }
    public void RotateTowards(Transform t)
    {
        Vector3 tPosWithoutY = new Vector3(t.position.x, thisTransform.position.y, t.position.z); //så den bara kollar på x o z leden
        Vector3 direction = (tPosWithoutY - thisTransform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        thisTransform.rotation = Quaternion.Slerp(thisTransform.rotation, lookRotation, Time.deltaTime * turnRatio);
    }

    public void ResetPath() //så att den inte fuckar när den är av, mer safe
    {
        if (agent != null)
        {
            if (IsActive() && agent.enabled == true && agent.isOnNavMesh == true)
            {
                agent.ResetPath();
            }
        }
    }
    public void SetDestination(Vector3 pos)
    {
        if (agent != null)
        {
            if (IsActive() && agent.enabled == true && agent.isOnNavMesh == true)
            {
                agent.SetDestination(pos);
            }
        }
    }

    public virtual bool LineOfSight() //has LOS to t?
    {
        RaycastHit hitLOS;
        //RaycastHit[] hitsLOS;
        Vector3 vectorToT = targetHealth.middlePoint - healthS.middlePoint; //hämta mittpunkten istället

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
            if (Physics.Raycast(healthS.middlePoint, vectorToT, out hitLOS, Mathf.Infinity, layerMaskLOSCheckFriendlyExcluded)) //ett layar som ignorerar allt förutom units o terräng
            {
                if (hitLOS.collider.gameObject.layer != LayerMask.NameToLayer("Terrain"))
                {
                    return true;
                }
            }
        }
        else //friendly target
        {
            if (Physics.Raycast(healthS.middlePoint, vectorToT, out hitLOS, Mathf.Infinity, layerMaskLOSCheck)) //ett layar som ignorerar allt förutom units o terräng
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
    public virtual bool LineOfSight(Transform t) //has LOS to t?
    {
        RaycastHit hitLOS;
        Vector3 vectorToT = t.GetComponent<Health>().middlePoint - healthS.middlePoint; //hämta mittpunkten istället

        if (!IsFriendly(target))
        {
            if (Physics.Raycast(healthS.middlePoint, vectorToT, out hitLOS, Mathf.Infinity, layerMaskLOSCheck)) //ett layar som ignorerar allt förutom units o terräng
            {
                if (hitLOS.collider.transform == t)
                {
                    return true;
                }
            }
        }
        else //target är friendly -> då får jag använda ett annat layer så jag hittar denne
        {
            if (Physics.Raycast(healthS.middlePoint, vectorToT, out hitLOS, Mathf.Infinity, layerMaskLOSCheckFriendlyExcluded)) //nu kommer friendlys oxå kunna blocka denne, tänk på det
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
