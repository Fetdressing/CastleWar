using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnBuilding : MonoBehaviour {
    private Transform thisTransform;

    public GameObject spawnUnit;
    public int maxUnitsOut = 5;
    public float spawnRate = 10;
    public Transform[] spawnPositions;
    public Transform rallyPointTransform;

    List<Unit> unitPool = new List<Unit>();
	// Use this for initialization
    public void Init()
    {
        thisTransform = this.transform;
        CreateObjectPool();

        StartCoroutine(Spawn());
    }

	void Start () {
        Init();
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
            temp.SetActive(false);
            unitPool.Add(tempUnit);
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
                //sätt den till samma lag som en själv!!!
                unitPool[i].obj.layer = thisTransform.gameObject.layer;
                break;
            }
        }
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
