using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(AgentStats))]
[RequireComponent(typeof(AudioSource))]
public class AgentBase : AIBase {
    [HideInInspector]
    public UnitState state;

    [HideInInspector]public Health healthS;
    [HideInInspector]public AgentStats statsS;
    [HideInInspector]public NavMeshAgent agent;
    [HideInInspector]public AudioSource audioSource;

    public float aggroDistance = 20;
    [HideInInspector]
    public float investigateDistance; //sätts när man börjar investigate:a
    [HideInInspector]
    public float temporaryAggroDistance = 0; //används när nått target skjuter långt
    //[HideInInspector]public List<Target> potTargets = new List<Target>(); //håller alla targets som kan vara, sen får man kolla vilka som kan nås och vilken aggro de har
    [HideInInspector]public AIBase targetBase;
    [HideInInspector]public Health targetHealth;
    [HideInInspector]public float targetDistance; //så jag inte behöver räkna om denna på flera ställen
    [HideInInspector]public bool isFriendlyTarget;

    public int nrAcceptedAttackersOnTarget = 3; //hur många andra som max attackerar mitt target för att jag inte ska vilja ta ett annat target

    [Header("Other")]
    //variables used in different states*****

    //almost reach target**
    public float reachDistanceThreshhold = 10; //hur nära som är 'close enough'
    bool closeToEnd = false;
    float timerReachEnd; //när denna nästan nått pathen så set en timer för att inte stå o jucka för all evighet, finns risk att denne inte kan nå target
    float reachEndPatience = 2.0f; //om den är hyfsat länge till slutpunkten så hur länge den ska vänta innan den ger upp och nöjer sig med nuvarande punkt    
    //almost reach target**

    //look angle threshhold, hur mycket den måste titta på fienden för att kunna attackera
    private float lookAngleThreshhold = 15;
    public float turnRatio = 2;

    public LayerMask layerMaskLOSCheck;
    [HideInInspector]
    public LayerMask layerMaskLOSCheckFriendlyExcluded; //samma som layerMaskLOSCheck fast MED sin egen layer

    [HideInInspector]
    public float startChaseTime;
    [HideInInspector]
    public float chaseTimeAggressive = 6;
    [HideInInspector]
    public float chaseTimeNormal = 8;

    Transform[] tempTargets; //används för att skanna nearby fiender tex

    [HideInInspector]public Vector3 startPos;
    [HideInInspector]public Vector3 startPos2; //används när man tex frångår pathen
    [HideInInspector]public Vector3 movePos; //för attackmove och move


    bool ignoreSurrounding = false; //används för att återta en path så att denne inte försöker jaga fiender hela tiden

    [Header("Animation")]
    public GameObject animationObject;
    [HideInInspector]
    public Animation animationH;

    public AnimationClip idle;
    public AnimationClip run;
    public float loopRunAnimSpeed = 0.4f;
    public float loopIdleAnimSpeed = 0.4f;
    public AnimationClip[] attackA;
    public float attackAnimSpeed = 0.3f; //speed of animation
    public float attack_applyDMG_Time = 0.2f; //när på attack animationen som skadan ska applyas
    [HideInInspector]
    public float attack_Timer_Begun;
    [HideInInspector]
    public bool isPerformingAttack = false;
    [HideInInspector]
    public bool hasAppliedDamage = false; //när skadan har skickats till fienden, så att den inte skickas flera gånger under samma animation
    [HideInInspector]
    public int lastAttackAnimIndex; //den animationen i listan som senast spelades

    [Header("Audio")]
    public AudioClip[] attackSounds;

    //variables used in different states*****

    // Use this for initialization
    void Start()
    { //får jag dubbla starts?
        //Init();
    }

    void Awake()
    {
        //Init();
    }

    public override void Reset()
    {
        base.Reset();
        closeToEnd = false;
        ignoreSurrounding = false;
        Guard();

        isPerformingAttack = false;
        hasAppliedDamage = false;
    }

    public override void Init()
    {
        base.Init();
        agent = thisTransform.GetComponent<NavMeshAgent>();
        healthS = thisTransform.GetComponent<Health>();
        statsS = thisTransform.GetComponent<AgentStats>();
        audioSource = thisTransform.GetComponent<AudioSource>();

        agent.angularSpeed = turnRatio * 110;

        if(animationObject != null)
        {
            animationH = animationObject.GetComponent<Animation>();

            animationH[run.name].speed = loopRunAnimSpeed;
            animationH[idle.name].speed = loopIdleAnimSpeed;
            for (int i = 0; i < attackA.Length; i++)
            {
                animationH[attackA[i].name].speed = attackAnimSpeed;
            }
        }

        InitializeStats();

        //Reset(); inte här
    }
    public override void InitializeStats()
    {
        base.InitializeStats();
        agent.speed = movementSpeed;
    }
    // Update is called once per frame
    void Update () {
        if (!healthS.IsAlive() && thisTransform.gameObject.activeSelf == false)
            return;
        
        UpdateEssentials();
        PlayStateAnimations();
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

    public override void UpdateEssentials()
    {
        base.UpdateEssentials();
        UpdateMoveSpeed();
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

        if(isPerformingAttack == true) //appliar skadan två gånger? varför??
        {
            //float modTime = animationH[attackA[lastAttackAnimIndex].name].time / animationH[attackA[lastAttackAnimIndex].name].length; //denna blir felet
            //Debug.Log(animationH[attackA[lastAttackAnimIndex].name].time.ToString());
            if (attack_applyDMG_Time <= (Time.time - attack_Timer_Begun)) //kolla ifall skadan ska applyas
            { // * 10 så den är i sekunder
                if (hasAppliedDamage == false)
                {

                    if (attackSounds.Length > 0)
                    {
                        int randomSound = Random.Range(0, attackSounds.Length);
                        audioSource.PlayOneShot(attackSounds[randomSound]);
                    }

                    hasAppliedDamage = true;
                    int damageRoll = RollDamage();

                    unitSpellHandler.RegisterAttack();
                    if (targetHealth.AddHealth(-damageRoll)) //target överlevde attacken
                    {
                        targetAlive = true;
                    }
                    else //target dog
                    {
                        targetAlive = false;
                    }
                }

                if (targetBase != null)
                {
                    targetBase.Attacked(thisTransform); //notera att jag attackerat honom!
                }
            }
            if(hasAppliedDamage == true) //kolla ifall animationen nästan är klar och så att den har gjort skada
            {
                //Debug.Log(animationH[attackA[lastAttackAnimIndex].name].time.ToString());
                isPerformingAttack = false; //jag vill inte snubben ska missa damage för att denne är för långsam med sin animation
            }
        }

        bool isFacingTarget = IsFacingTransform(target);

        if (attackRange > (targetDistance-targetHealth.unitSize) && isFacingTarget && isPerformingAttack == false) //kolla så att target står framför mig oxå
        {
            if (attackSpeedTimer <= Time.time)
            {
                isPerformingAttack = true; //påbörja attacken
                hasAppliedDamage = false;
                lastAttackAnimIndex = Random.Range(0, attackA.Length);
                //animationH[attackA[lastAttackAnimIndex].name].layer = 1;
                //animationH[attackA[lastAttackAnimIndex].name].weight = 1;
                animationH[attackA[lastAttackAnimIndex].name].time = 0.0f;
                animationH.Play(attackA[lastAttackAnimIndex].name);
                attackSpeedTimer = attackSpeed + Time.time;
                attack_Timer_Begun = Time.time;        
            }
        }
        if (attackRange * 0.8f < (targetDistance-targetHealth.unitSize)) //marginal med jue
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
        base.Attacked(attacker);

        if(attacker.GetComponent<AIBase>().GetNrAttackers() > nrAcceptedAttackersOnTarget) //pallar inte bry mig om den redan har massa targets
        { //har target dock väldigt få attackers på sig så kan jag oxå stanna
            return;
        }

        if(target != null && targetBase != null && targetBase.GetNrAttackers() < nrAcceptedAttackersOnTarget) //det target jag har nu är nog inte så fel
        {
            return;
        }

        if (state != UnitState.AttackingUnit && state != UnitState.Moving && attacker != null)
        {
            bool validTarget = true;
            for (int i = 0; i < friendlyLayers.Count; i++) //kolla så att man inte råkar göra en friendly till target
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
                if (target == null || !isUnitAttackingMe(target)) //attackera hellre det targetet som attackerar mig än det som inte gör de
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
        target = null;
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
                investigateDistance = GetDistanceToPosition(pos) * 1.3f;
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
                NewTarget(GetBestTarget(tempTargets));
            }
            else
            {
                if (GetStartPointDistance() > 1.5f || !IsCloseEnoughToPos(startPos)) //så att den inte ska jucka
                {
                    SetDestination(startPos);
                }
            }
        }

        if (target != null)
        {
            if (GetTargetDistance() > attackRange * 1.3f) //check for closer target
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
                    AttackMove(startPos); //hm kan bli knas om den bara flyttar längre o längre ut
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
        if (target == null)
        {
            tempTargets = ScanEnemies(aggroDistance);
            if (tempTargets != null && tempTargets.Length != 0)
            {
                NewTarget(GetBestTarget(tempTargets));
                startPos2 = thisTransform.position; //därifrån den börja jaga, så att den kan återupta sin path efter den jagat
            }
            else //inga targets, återvänd till pathen typ
            {
                SetDestination(movePos);
            }
        }

        if (GetDistanceToPosition(movePos) < 1.5f) //kom fram
        {
            ExecuteNextCommand();
            return;
        }
        else if (target == null && IsCloseEnoughToPos(movePos)) //den får inte vara klar bara för att den råkar passera punkten när den jagar ett target
        {
            ExecuteNextCommand();
            return;
        }

        if (target != null)
        {
            if(GetTargetDistance() > attackRange * 1.3f) //check for closer target
            {
                Transform potTarget = CheckForBetterTarget(attackRange);
                if(potTarget != null)
                {
                    NewTarget(potTarget);
                }
            }

            bool continueChaseTarget = true; //flyttar ju inte på sig efter de dödat target??
            if (GetTargetDistance() < aggroDistance * 1.05f)
            {
                if ((startChaseTime + chaseTimeNormal) > Time.time)
                {
                    if (AttackTarget() == false)
                    {
                        target = null;
                        //AttackMove(movePos);
                    }
                }
                else continueChaseTarget = false;
            }
            else continueChaseTarget = false;

            if(continueChaseTarget == false)
            {
                target = null;
                SetDestination(movePos);
            }
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

    public virtual void InvestigatingUpdate()
    {
        if (target == null && ignoreSurrounding == false)
        {
            tempTargets = ScanEnemies(aggroDistance);
            if (tempTargets != null && tempTargets.Length != 0)
            {
                NewTarget(GetBestTarget(tempTargets));
                startPos2 = thisTransform.position; //därifrån den börja jaga, så att den kan återupta sin investigation efter den jagat
            }
            else //hittade inget target, så fortsätt med mitt mål
            {
                SetDestination(movePos);
                if(HasReachedPosition(movePos)) //återvänd hem igen för jag kom fram
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
            
            bool continueChaseTarget = true;
            if (AttackTarget() == false)
            {
                continueChaseTarget = false;
            }
            if (GetStartPointDistance2() > investigateDistance) //måste kunna återvända till där den var innan den påbörja pathen
            {
                if ((startChaseTime + chaseTimeNormal) < Time.time) //denna skulle kunna påbörjas när den går utanför investigateDistance
                {
                    continueChaseTarget = false;
                }
            }
            
            if(continueChaseTarget == false)
            {
                target = null;
                SetDestination(startPos2);
                ignoreSurrounding = true;
            }
        }
        else
        {
            if (ignoreSurrounding && IsCloseEnoughToPos(startPos2)) //kommit tillbaks till pathen -> fortsätt investigate!
            {
                ignoreSurrounding = false;
                target = null;
            }
            else if (HasReachedPosition(movePos)) //så att den inte ska jucka, när jag nått target så dra hem igen!
            {
                //Debug.Log("GO home!");
                Move(startPos);
            }
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
    public virtual Transform GetBestTarget(Transform[] ts) //om ts är sorterad efter avstånd så kommer den kolla de närmre fienderna först
    {
        Transform tTemp = null;
        for (int i = 0; i < ts.Length; i++)
        {
            if (ts[i].GetComponent<AIBase>().GetNrAttackers() < nrAcceptedAttackersOnTarget)
            {
                tTemp = ts[i];
                break;
            }
        }
        if (tTemp == null)
        {
            int randomIndex = Random.Range(0, ts.Length);
            tTemp = ts[randomIndex];
        }

        return tTemp;
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
        if(GetNrAttackers() > 0)
        {
            return null;
        }
        Transform[] potTargets = ScanEnemies(attackRange);
        if(potTargets == null)
        {
            return null;
        }
        //SortTransformsByDistance(ref potTargets);
        //Transform tTemp = ClosestTransform(potTargets); den är redan närmst på plats 0 i potTargets

        if (GetTargetDistance() > GetDistanceToTransform(potTargets[0]))
        {
            return potTargets[0];
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
    public bool HasReachedPosition(Vector3 pos) //krävs att man har en path
    {
        agent.SetDestination(pos);
        if (GetDistanceToPosition(pos) < 1.5f)
        {
            return true;
        }
        if (agent.pathPending) //så agent.remainingDistance funkar (den är inte klar med o beräkna så vänta lite)
        {
            return false;
        }
        if (agent.remainingDistance < 1.5f)
        {
            return true;
        }
        return false;
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

    public void PlayStateAnimations()
    {
        if (agent.hasPath == true)
        {
            animationH[run.name].weight = 0.2f;
            animationH[run.name].layer = 10;
            animationH.CrossFade(run.name);
        }
        else
        {
            animationH[idle.name].weight = 0.2f;
            animationH[idle.name].layer = 10;
            animationH.CrossFade(idle.name);
        }
    }

    public override void UpdateMoveSpeed()
    {
        agent.speed = movementSpeed;
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

//public virtual void AttackMovingUpdate()
//{
//    if (target == null && ignoreSurrounding == false)
//    {
//        tempTargets = ScanEnemies(aggroDistance);
//        if (tempTargets != null && tempTargets.Length != 0)
//        {
//            NewTarget(GetBestTarget(tempTargets));
//            startPos2 = thisTransform.position; //därifrån den börja jaga, så att den kan återupta sin path efter den jagat
//        }
//        else //inga targets, återvänd till pathen typ
//        {
//            SetDestination(movePos);
//        }
//    }


//    if (target != null)
//    {
//        if (GetTargetDistance() > attackRange * 1.5f) //check for closer target
//        {
//            Transform potTarget = CheckForBetterTarget(attackRange);
//            if (potTarget != null)
//            {
//                NewTarget(potTarget);
//            }
//        }

//        bool continueChaseTarget = true; //flyttar ju inte på sig efter de dödat target??
//        if (GetTargetDistance() < aggroDistance * 1.05f)
//        {
//            if ((startChaseTime + chaseTimeNormal) > Time.time)
//            {
//                if (AttackTarget() == false)
//                {
//                    target = null;
//                    //AttackMove(movePos);
//                }
//            }
//            else continueChaseTarget = false;
//        }
//        else continueChaseTarget = false;

//        if (continueChaseTarget == false)
//        {
//            target = null;
//            SetDestination(startPos2); //återvänd till pathen
//            ignoreSurrounding = true;
//        }
//    }

//    if (ignoreSurrounding && HasReachedPosition(startPos2) || IsCloseEnoughToPos(startPos2)) //kommit tillbaks till pathen -> fortsätt attackmove!
//    {
//        ignoreSurrounding = false;
//        //AttackMove(movePos);
//    }

//    if (ignoreSurrounding == false) //så att agent.remainingDistance inte går igång för någon annan position än attackmove positionen
//    {
//        if (HasReachedPosition(movePos)) //kom fram
//        {
//            ExecuteNextCommand();
//        }
//        else if (target == null && IsCloseEnoughToPos(movePos)) //den får inte vara klar bara för att den råkar passera punkten när den jagar ett target
//        {
//            ExecuteNextCommand();
//        }
//    }
//}
