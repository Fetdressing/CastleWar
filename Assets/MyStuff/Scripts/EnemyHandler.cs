using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyHandler : MonoBehaviour {
    TeamHandler teamHandler;

    public Transform[] enemyTargetPoints;

    GameObject[] registeredEnemies;
    AIBase[] registeredEnemiesAIBase;

    public void Init()
    {
        teamHandler = GameObject.FindGameObjectWithTag("TeamHandler").GetComponent<TeamHandler>();
        StartCoroutine(EnemyUpdate());
    }

    IEnumerator EnemyUpdate()
    {
        while (this != null)
        {
            //order move för fiender
            RegisterEnemies();
            for(int i = 0; i < registeredEnemiesAIBase.Length; i++)
            {
                registeredEnemiesAIBase[i].AttackMove(enemyTargetPoints[0].position);
            }
            yield return new WaitForSeconds(15);
        }
    }

    void RegisterEnemies()
    {
        string[] enemyTeams = teamHandler.GetPlayerTeam().enemyTeams;
        List<int> enemyLayers = new List<int>();
        for(int i = 0; i < enemyTeams.Length; i++)
        {
            enemyLayers.Add(LayerMask.NameToLayer(enemyTeams[i]));
        }
        registeredEnemies = FindGameObjectsWithLayer(enemyLayers);
        registeredEnemiesAIBase = new AIBase[registeredEnemies.Length];
        for (int i = 0; i < registeredEnemies.Length; i++)
        {
            registeredEnemiesAIBase[i] = registeredEnemies[i].GetComponent<AIBase>();
        }
    }

    GameObject[] FindGameObjectsWithLayer(List<int> layers)
    {
        GameObject[] goArray = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        List<GameObject> goList = new List<GameObject>();
        for (int i = 0; i < goArray.Length; i++)
        {
            for(int y = 0; y < layers.Count; y++)
            {
                if (goArray[i].layer == layers[y])
                {
                    goList.Add(goArray[i]);
                    break;
                }
            }
        }
        if (goList.Count == 0)
        {
            return null;
        }
        return goList.ToArray();
    }

}
