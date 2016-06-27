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

            Unit tempUnit = new Unit(temp, agentBase, unitHealth);

            temp.layer = thisTransform.gameObject.layer;
            agentBase.GetFriendsAndFoes();
            temp.transform.parent = thisTransform;
            temp.SetActive(false);
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
                    unitPool[i].agentB.Move(unitPool[i].obj.transform.position + new Vector3(0,0, 1)); //move så de inte fastnar inuti varandra
                    //unitPool[i].agentB.Guard();
                }
                break;
            }
        }
    }
    //tomma referenser när denna dör
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
