using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class AIBase : MonoBehaviour {
    [HideInInspector]
    public static long nrAI = 0;
    public long id; //denna går inte att ändras från editor, eller den skrivs över ändå. Bara för att ha koll på den
    [HideInInspector]
    public Transform thisTransform;

    [HideInInspector]
    public List<string> friendlyLayers = new List<string>(); //bra att dessa är strings, behövs till tex AgentRanged
    [HideInInspector]
    public string[] enemyLayers;

    [HideInInspector]
    public LayerMask friendlyOnly;
    [HideInInspector]
    public LayerMask enemyOnly;

    public enum UnitState { AttackMoving, Moving, Guarding, Investigating, AttackingUnit };

    public struct Command
    {
        public UnitState stateToExecute;
        public Vector3 positionToExecute; //använd sedan thisTransform.position för start ofc
        public Transform target;
        public bool friendlyFire;

        public Command(UnitState uS, Vector3 pos, Transform t, bool ff)
        {
            stateToExecute = uS;
            positionToExecute = pos;
            target = t;
            friendlyFire = ff;
        }
    }

    [HideInInspector]
    public List<Command> nextCommando = new List<Command>(); //kedje kommandon??

    [HideInInspector]
    public List<long> attackerIDs = new List<long>(); //används för o mecka target tex

    public virtual void Init()
    {
        id = nrAI;
        nrAI++;
        thisTransform = this.transform;
        GetFriendsAndFoes();
        ClearCommands();
        attackerIDs.Clear();
    }

    public virtual void Reset()
    {
        attackerIDs.Clear();
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


    public virtual void AttackMove(Vector3 pos) { }

    public virtual void Move(Vector3 pos) { }

    public virtual void Guard() { }

    public virtual void Guard(Vector3 pos) { }

    public virtual void Investigate(Vector3 pos) { }

    public virtual void AttackUnit(Transform t, bool friendlyFire) { }


    public virtual void ExecuteNextCommand() { }

    public virtual void AddCommandToList(Vector3 pos, UnitState nextState, Transform tar, bool friendlyfire) { }

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
            if (hitColliders[i].transform != thisTransform) //vill väl inte ha mig själv i listan?
            {
                hits.Add(hitColliders[i].transform);
            }
        }
        return hits;
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

        if (hitColliders.Length > 0)
        {
            for (int i = 0; i < hitColliders.Length; i++)
            {
                hits[i] = hitColliders[i].transform;
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
    public virtual void SortTransformsByDistance(ref Transform[] ts) //index 0 kommer hamna närmst
    {
        List<Transform> tempTs = new List<Transform>();
        for (int i = 0; i < ts.Length; i++)
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

        for (int i = 0; i < ts.Length; i++)
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
                    yield break;
                }
            }
        
            //den fanns inte redan! lägg till den
            attackerIDs.Add(attackerID);
            //int savedIndex = attackerIDs.Count;
            yield return new WaitForSeconds(5);
            attackerIDs.Remove(attackerID);
        }
    }
    public int GetNrAttackers()
    {
        return attackerIDs.Count;
    }
}
