using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyHandler : MonoBehaviour {
    TeamHandler teamHandler;

    public Transform[] enemyTargetPoints;
    public int splitTargetIndex = 3; //hur ofta den ska köra en ny random på targetPositioner för listan av units

    GameObject[] registeredEnemies;
    AIBase[] registeredEnemiesAIBase;

    public void Init() //gör stöd för att kunna lägga in speciella events, typ skicka massa skit vid minut 10
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
            if ((registeredEnemiesAIBase.Length / splitTargetIndex) > 0)
            {
                for (int i = 0; i < registeredEnemiesAIBase.Length; i++)
                {
                    int tPointIndex = 0;
                    if (i % (registeredEnemiesAIBase.Length / splitTargetIndex) == 0)
                    {
                        int tries = 0; //så den inte ska försöka förevigt
                        do
                        {
                            tPointIndex = Random.Range(0, enemyTargetPoints.Length);
                            tries++;
                        } while (enemyTargetPoints[tPointIndex] != null && tries < (splitTargetIndex * 3));
                    }
                    if (enemyTargetPoints[tPointIndex] != null)
                    {
                        registeredEnemiesAIBase[i].AttackMove(enemyTargetPoints[tPointIndex].position);
                    }
                }
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
        if (registeredEnemies != null)
        {
            registeredEnemiesAIBase = new AIBase[registeredEnemies.Length];
            for (int i = 0; i < registeredEnemies.Length; i++)
            {
                registeredEnemiesAIBase[i] = registeredEnemies[i].GetComponent<AIBase>();
            }
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
