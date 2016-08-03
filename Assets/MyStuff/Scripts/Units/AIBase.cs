﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[RequireComponent(typeof(UnitSpellHandler))]
public abstract class AIBase : MonoBehaviour {
    [HideInInspector]
    public int initializedTimes = 0;
    [HideInInspector]
    public static long nrAI = 0;
    public long id; //denna går inte att ändras från editor, eller den skrivs över ändå. Bara för att ha koll på den
    [HideInInspector]
    public Transform thisTransform;
    [HideInInspector]
    public Transform target;
    public GameObject animationObject;

    [HideInInspector]
    public List<string> friendlyLayers = new List<string>(); //bra att dessa är strings, behövs till tex AgentRanged
    [HideInInspector]
    public string[] enemyLayers;

    [HideInInspector]
    public LayerMask friendlyOnly;
    [HideInInspector]
    public LayerMask enemyOnly;

    public enum UnitState { AttackMoving, Moving, Guarding, Investigating, AttackingUnit, CastingSpell };

    public struct Command
    {
        public UnitState stateToExecute;
        public Vector3 positionToExecute; //använd sedan thisTransform.position för start ofc
        public Transform target;
        public bool friendlyFire;
        public int spellIndex;

        public Command(UnitState uS, Vector3 pos, Transform t, bool ff, int spellindex)
        {
            stateToExecute = uS;
            positionToExecute = pos;
            target = t;
            friendlyFire = ff;
            spellIndex = spellindex;
        }
    }

    [HideInInspector]
    public List<Command> nextCommando = new List<Command>(); //kedje kommandon??

    [HideInInspector]
    public List<long> attackerIDs = new List<long>(); //används för o mecka target tex

    //stats****
    [Header("Stats")]
    public int startMovementSpeed = 10;
    [HideInInspector]
    public int movementSpeed;

    public TypeDamage damageType;
    public int startDamageSpread = 3;
    public int startDamageDamage = 6;
    [HideInInspector]
    public int damageSpread;
    [HideInInspector]
    public int damage;

    public int attackFatigueCost = 10;
    public float minFatigueAffection = 0.3f; //hur mycket fatigued påverkar som mest, alltså 30% av attacken ifall currFatigue = 0

    public float startAttackSpeed = 1.2f;
    [HideInInspector]
    public float attackSpeed; //public så att agentStats kan påverka den
    [HideInInspector]
    public float attackSpeedTimer = 0.0f;

    public float startAttackRange = 30;
    [HideInInspector]
    public float attackRange;

    //public float startSpellRange = 30;
    //[HideInInspector]
    //public float spellRange;
    [HideInInspector]
    public UnitSpellHandler unitSpellHandler;
    [HideInInspector]
    public int validSpellIndexMax = 0; //spell indexes somm denna unit har som högst, dvs 4 är allra högst (q,w,e,r)
    [HideInInspector]
    public int currSpellIndex = 0;
    public bool autocast = false;

    public int startFatigue = 100;
    [HideInInspector]
    public int maxFatigue;
    [HideInInspector]
    public int currFatigue;
    private float fatigueRegIntervall = 0.8f;
    private float fatigueRegTimer = 0.0f;
    public int startFatigueReg = 3;
    [HideInInspector]
    public int fatigueReg;
    public Image fatigueBar;

    public virtual void Init()
    {
        initializedTimes++;
        id = nrAI;
        nrAI++;
        thisTransform = this.transform;
        unitSpellHandler = thisTransform.GetComponent<UnitSpellHandler>();
        GetFriendsAndFoes();
        ClearCommands();
        attackerIDs.Clear();
        LoadValidSpellIndexes();
    }

    public virtual void Reset()
    {
        StopAllCoroutines();
        attackerIDs.Clear();
        fatigueRegTimer = 0.0f;
        InitializeStats();
    }

    public virtual void Dealloc() { }
    public void GetFriendsAndFoes()
    {
        TeamHandler th = GameObject.FindGameObjectWithTag("TeamHandler").gameObject.GetComponent<TeamHandler>();
        th.GetFriendsAndFoes(LayerMask.LayerToName(this.transform.gameObject.layer), ref friendlyLayers, ref enemyLayers);
        InitializeLayerMask();
    }
    public void InitializeLayerMask()
    {
        friendlyOnly = LayerMask.NameToLayer("Nothing"); //inte riktigt säker på varför detta funkar men det gör
        enemyOnly = LayerMask.NameToLayer("Nothing");

        friendlyOnly = ~friendlyOnly; //inte riktigt säker på varför detta funkar men det gör
        enemyOnly = ~enemyOnly;
        //friendlyOnly |= (1 << thisTransform.gameObject.layer);
        bool alreadyExist = false;
        for (int i = 0; i < friendlyLayers.Count; i++)
        {
            if (LayerMask.LayerToName(this.transform.gameObject.layer) == friendlyLayers[i])
            {
                alreadyExist = true;
            }
        }
        if (alreadyExist == false)
        {
            friendlyLayers.Add(LayerMask.LayerToName(this.transform.gameObject.layer));
        }


        for (int i = 0; i < friendlyLayers.Count; i++)
        {
            friendlyOnly |= (1 << LayerMask.NameToLayer(friendlyLayers[i]));
        }


        for (int i = 0; i < enemyLayers.Length; i++)
        {
            bool isValid = true;
            for (int y = 0; y < friendlyLayers.Count; y++) //kolla så att den inte är en friendly oxå
            {
                if (LayerMask.NameToLayer(enemyLayers[i]) == LayerMask.NameToLayer(friendlyLayers[y]))
                {
                    isValid = false;
                }
            }

            if (LayerMask.NameToLayer(enemyLayers[i]) == this.transform.gameObject.layer) //kolla så att det inte är mitt eget layer
            {
                isValid = false;
            }

            if (isValid == true)
            {
                enemyOnly |= (1 << LayerMask.NameToLayer(enemyLayers[i]));
            }
        }

    }
    public virtual void InitializeStats() //ha med andra påverkande faktorer här sedan
    {
        movementSpeed = startMovementSpeed;

        damage = startDamageDamage;
        damageSpread = startDamageSpread;

        attackSpeed = startAttackSpeed;

        attackRange = startAttackRange;

        maxFatigue = startFatigue;
        currFatigue = maxFatigue;
        fatigueReg = startFatigueReg;
    }
    public void LoadValidSpellIndexes()
    {
        if (unitSpellHandler == null) return;
        validSpellIndexMax = unitSpellHandler.allAbilities.Count;
    }


    public virtual void AttackMove(Vector3 pos) { }

    public virtual void Move(Vector3 pos) { }

    public virtual void Guard() { }

    public virtual void Guard(Vector3 pos) { }

    public virtual void Investigate(Vector3 pos) { }

    public virtual void PerformSpell(Vector3 pos, int spellIndex)
    {
        currSpellIndex = spellIndex;
    }

    public virtual void AttackUnit(Transform t, bool friendlyFire) { }

    public virtual bool CastSpell(Vector3 pos, int spellIndex, ref bool isCastable, ref int currFatigue)
    {
        return true;
    } //försöker kasta spellen

    public virtual void AutoCastSpell() //används i typ attack funktionen
    {
        if (unitSpellHandler == null) return;
        if (target.GetComponent<AIBase>().IsFriendly(thisTransform)) return;
        if (autocast && validSpellIndexMax > 0)
        {
            int randomSpellIndex = Random.Range(0, validSpellIndexMax);
            bool isCastable = false;
            unitSpellHandler.CastSpell(target.position, randomSpellIndex, ref isCastable, ref currFatigue);
        }
    }

    public virtual void ExecuteNextCommand() { }

    public virtual void AddCommandToList(Vector3 pos, UnitState nextState, Transform tar, bool friendlyfire, int spellIndex) { }

    public virtual void ClearCommands()
    {
        nextCommando.Clear();
    }


    public virtual void Attacked(Transform attacker) //blev attackerad av någon
    {
        if (attacker != null && attacker.gameObject.activeSelf == true && thisTransform != null && thisTransform.gameObject.activeSelf == true)
        {
            StartCoroutine(AddAttacker(attacker));
        }
    }
    public virtual void NotifyNearbyFriendly(Vector3 pos)
    {
        List<Transform> nearbyFriendly = ScanFriendlys(15); //standard värde bara

        for (int i = 0; i < nearbyFriendly.Count; i++)
        {
            if (nearbyFriendly[i].gameObject.activeSelf == true && nearbyFriendly[i].GetComponent<AgentBase>() != null)
            {
                nearbyFriendly[i].GetComponent<AgentBase>().Investigate(pos); //behöver något annat än attackmove, investigate
            }
        }
        
    }
    public virtual List<Transform> ScanFriendlys(float aD)
    {
        Collider[] hitColliders = Physics.OverlapSphere(thisTransform.position, aD, friendlyOnly);
        //int i = 0;
        //while (i < hitColliders.Length)
        //{
        //    Debug.Log(hitColliders[i].transform.name);
        //    i++;
        //}
        List<Transform> hits = new List<Transform>();
        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (hitColliders[i].transform != thisTransform && hitColliders[i].transform.GetComponent<Health>().IsAlive() == true) //vill väl inte ha mig själv i listan?
            {
                hits.Add(hitColliders[i].transform);
            }
        }
        return hits;
    }
    public virtual List<Transform> ScanEnemies(float aD)
    {
        Collider[] hitColliders = Physics.OverlapSphere(thisTransform.position, aD, enemyOnly);
        //int i = 0;
        //while (i < hitColliders.Length)
        //{
        //    Debug.Log(hitColliders[i].transform.name);
        //    i++;
        //}
        List<Transform> hits = new List<Transform>();

        if (hitColliders.Length > 0)
        {
            for (int i = 0; i < hitColliders.Length; i++)
            {
                if (hitColliders[i].transform.GetComponent<Health>().IsAlive() == true)
                {
                    hits.Add(hitColliders[i].transform);
                }
            }
            if (thisTransform.GetComponent<AgentBase>() != null)
            {
                SortTransformsByDistance(ref hits); //index 0 kommer hamna närmst
            }
            return hits;
        }
        else return null;
    }
    public bool isUnitAttackingMe(Transform t)
    {
        if (t != null)
        {
            if (t.GetComponent<AIBase>() != null)
            {
                long idT = t.GetComponent<AIBase>().id;
                for (int i = 0; i < attackerIDs.Count; i++)
                {
                    if (idT == attackerIDs[i]) //den fanns i listan = den attackerar mig
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        return false;
    }

    public bool IsFriendly(Transform t)
    {
        string tLayer = LayerMask.LayerToName(t.gameObject.layer);

        for (int i = 0; i < friendlyLayers.Count; i++)
        {
            if (tLayer == friendlyLayers[i])
            {
                return true;
            }
        }

        return false;
    }

    public float GetDistanceToTransform(Transform t)
    {
        return Vector3.Distance(thisTransform.position, t.position);
    }
    public float GetDistanceToPosition(Vector3 p)
    {
        return Vector3.Distance(thisTransform.position, p);
    }
    public virtual void SortTransformsByDistance(ref List<Transform> ts) //index 0 kommer hamna närmst
    {
        List<Transform> tempTs = new List<Transform>();
        for (int i = 0; i < ts.Count; i++)
        {
            tempTs.Add(ts[i]);
        }
        //Transform closest = transforms[0];
        tempTs.Sort(delegate (Transform a, Transform b)
        {
            return Vector3.Distance(thisTransform.position, a.transform.position)
            .CompareTo(
              Vector3.Distance(thisTransform.position, b.transform.position));
        });

        for (int i = 0; i < ts.Count; i++)
        {
            ts[i] = tempTs[i]; //senare index hamnar längre ifrån, 0 är närmst
            //Debug.Log(thisTransform.name + " Index: " + i.ToString() + Vector3.Distance(ts[i].position, thisTransform.position).ToString());
        }

    }

    public virtual bool IsActive() //kolla ifall denna är aktiv
    {
        if (thisTransform != null)
        {
            if (thisTransform.gameObject.activeSelf == true)
            {
                return true;
            }
        }
        return false;
    }

    IEnumerator AddAttacker(Transform attacker)
    {
        if (attacker != null && attacker.gameObject.activeSelf == true)
        {
            long attackerID = attacker.GetComponent<AIBase>().id;
            for (int i = 0; i < attackerIDs.Count; i++)
            {
                if (attackerID == attackerIDs[i])
                {
                    yield break; //returnerar I guess
                }
            }
        
            //den fanns inte redan! lägg till den
            attackerIDs.Add(attackerID);
            //int savedIndex = attackerIDs.Count;
            while(attacker != null && attacker.GetComponent<AIBase>().target == thisTransform) //fortfarande på mig
            {
                yield return new WaitForSeconds(5);
            }
            //måste kolla ifall den fortfarande attackera mig

            attackerIDs.Remove(attackerID);
        }
    }
    public int GetNrAttackers()
    {
        return attackerIDs.Count;
    }

    public virtual int RollDamage()
    {
        float fatigueAffection = Mathf.Clamp((float)currFatigue / (float)maxFatigue, minFatigueAffection, 1.0f);
        return ((int)(Random.Range(damage - damageSpread, damage + damageSpread) * fatigueAffection));
    }
    public int GetMinDamage()
    {
        return damage - damageSpread;
    }
    public int GetMaxDamage()
    {
        return damage + damageSpread;
    }

    public virtual void UpdateEssentials()
    {
        if(fatigueRegTimer < Time.time)
        {
            fatigueRegTimer = Time.time + fatigueRegIntervall;
            AddFatigue(fatigueReg);
        }
    }
    public virtual void UpdateMoveSpeed(){}

    public virtual bool AddFatigue(int f)
    {
        bool isFatigued = false;
        currFatigue += f;
        if(currFatigue < 0)
        {
            currFatigue = 0;
            isFatigued = true;
        }

        if(currFatigue > maxFatigue)
        {
            currFatigue = maxFatigue;
        }

        if(currFatigue >= maxFatigue)
        {
            fatigueBar.transform.parent.gameObject.SetActive(false);
        }
        else if(fatigueBar.transform.parent.gameObject.activeSelf == false)
        {
            fatigueBar.transform.parent.gameObject.SetActive(true);
        }

        fatigueBar.fillAmount = (float)currFatigue / (float)maxFatigue;
        return isFatigued; //true att fatiguen är slut
    }
}
