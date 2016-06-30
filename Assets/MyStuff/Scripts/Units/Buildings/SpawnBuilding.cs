using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnBuilding : BuildingBase {
    public GameObject spawnUnit;
    public int maxUnitsOut = 5;
    public float spawnRate = 10;
    public Transform[] spawnPositions;
    public Transform rallyPointTransform;

    List<Unit> unitPool = new List<Unit>();
	// Use this for initialization
    public override void Init()
    {
        base.Init();
        CreateObjectPool();

        StartCoroutine(Spawn());
    }

    public override void Dealloc()
    {
        base.Dealloc();
        for(int i = 0; i < unitPool.Count; i++)
        {
            unitPool[i].agentB.Dealloc();
            Destroy(unitPool[i].obj);
        }
    }

    void Start () {
        //Init();
	}

    void Awake()
    {
        Init();
    }

    void CreateObjectPool()
    {
        for (int i = 0; i < maxUnitsOut; i++)
        {
            GameObject temp = Instantiate(spawnUnit.gameObject);
            AgentBase agentBase = temp.GetComponent<AgentBase>();
            Health unitHealth = temp.GetComponent<Health>();

            temp.layer = thisTransform.gameObject.layer;
            //agentBase.GetFriendsAndFoes();
            agentBase.Init();
            //temp.transform.parent = thisTransform;
            temp.SetActive(false);

            Unit tempUnit = new Unit(temp, agentBase, unitHealth);
            unitPool.Add(tempUnit);

            //GameObject temp = Instantiate(spawnUnit.gameObject);
            //Transform unitTransform = temp.transform.transform.GetChild(0); //första barnet
            //AgentBase agentBase = unitTransform.GetComponent<AgentBase>();
            //Health unitHealth = unitTransform.GetComponent<Health>();
            //unitTransform.gameObject.layer = thisTransform.gameObject.layer;

            //Unit tempUnit = new Unit(temp, agentBase, unitHealth);

            //agentBase.GetFriendsAndFoes();
            //temp.transform.parent = thisTransform;
            //temp.SetActive(false);
            //unitPool.Add(tempUnit);
        }
    }

    IEnumerator Spawn()
    {
        while(thisTransform != null)
        {
            SpawnUnit();
            yield return new WaitForSeconds(spawnRate);
        }
    }

    void SpawnUnit()
    {
        for(int i = 0; i < unitPool.Count; i++)
        {
            if(unitPool[i].obj.activeSelf == false)
            {
                unitPool[i].obj.transform.position = spawnPositions[Random.Range(0, spawnPositions.Length)].position;
                unitPool[i].obj.SetActive(true);
                unitPool[i].agentB.Reset();
                unitPool[i].health.Reset();
                if(rallyPointTransform != null)
                {
                    unitPool[i].agentB.AttackMove(rallyPointTransform.position);
                }
                else
                {
                    unitPool[i].agentB.Move(unitPool[i].obj.transform.position + new Vector3(0, 0, 1)); //move så de inte fastnar inuti varandra
                    //unitPool[i].agentB.Guard();
                }
                break;
            }
        }
    }
    //tomma referenser när denna dör

    public override void AttackMove(Vector3 pos)
    {
        if(rallyPointTransform != null)
        {
            rallyPointTransform.position = pos;
        }
    }

    public override void Move(Vector3 pos)
    {
        if (rallyPointTransform != null)
        {
            rallyPointTransform.position = pos;
        }
    }

    public override void AttackUnit(Transform t, bool friendlyFire)
    {
        if (rallyPointTransform != null)
        {
            rallyPointTransform.position = t.position;
        }
    }

    public override void AddCommandToList(Vector3 pos, UnitState nextState, Transform tar, bool friendlyfire) //den ska ju bara kunna attackera units så denna behöver moddas
    {
        AttackMove(pos);
        //Command c = new Command(nextState, pos, tar, friendlyfire);
        //if (nextCommando.Count > 5) //vill inte göra denna lista hur lång som helst
        //{
        //    nextCommando[nextCommando.Count - 1] = c; //släng på den på sista platsen
        //    return;
        //}
        //nextCommando.Add(c);
    }

}

public struct Unit
{
    public GameObject obj;
    public AgentBase agentB;
    public Health health;

    public Unit(GameObject unitObject, AgentBase unitAgentBase, Health unitHealth)
    {
        obj = unitObject;
        agentB = unitAgentBase;
        health = unitHealth;
    }
}
