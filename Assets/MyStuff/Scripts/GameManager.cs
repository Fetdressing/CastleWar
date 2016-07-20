using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {
    TeamHandler teamHandler;
    Selector selector;
    Builder builder;
    EnemyHandler enemyHandler;
	// Use this for initialization
	void Start () {
        //denna ordningen är ganska viktig
        teamHandler = GameObject.FindGameObjectWithTag("TeamHandler").GetComponent<TeamHandler>();
        selector = GameObject.FindGameObjectWithTag("PlayerHandler").GetComponent<Selector>();
        builder = GameObject.FindGameObjectWithTag("PlayerHandler").GetComponent<Builder>();
        enemyHandler = GameObject.FindGameObjectWithTag("EnemyHandler").GetComponent<EnemyHandler>();

        teamHandler.Init();
        selector.Init();
        builder.Init();
        enemyHandler.Init();
    }
	
}
