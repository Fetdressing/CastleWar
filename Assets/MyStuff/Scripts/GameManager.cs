using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {
    TeamHandler teamHandler;
    Selector selector;
    Builder builder;
    EnemyHandler enemyHandler;
    AbilityManager abilityManager;
	// Use this for initialization
	void Start () {
        //denna ordningen är ganska viktig
        teamHandler = GameObject.FindGameObjectWithTag("TeamHandler").GetComponent<TeamHandler>();
        selector = GameObject.FindGameObjectWithTag("PlayerHandler").GetComponent<Selector>();
        builder = GameObject.FindGameObjectWithTag("PlayerHandler").GetComponent<Builder>();
        enemyHandler = GameObject.FindGameObjectWithTag("EnemyHandler").GetComponent<EnemyHandler>();
        abilityManager = GameObject.FindGameObjectWithTag("AbilityManager").GetComponent<AbilityManager>();

        teamHandler.Init();
        selector.Init();
        builder.Init();
        enemyHandler.Init();
        abilityManager.Init();

        Health[] healths = (Health[])FindObjectsOfType(typeof(Health));
        for (int i = 0; i < healths.Length; i++)
        {
            healths[i].Init();
        }
        AIBase[] aiBases = (AIBase[])FindObjectsOfType(typeof(AIBase));
        for(int i = 0; i < aiBases.Length; i++)
        {
            aiBases[i].Init();
        }
        UnitSpellHandler[] unitSH = (UnitSpellHandler[])FindObjectsOfType(typeof(UnitSpellHandler));
        for (int i = 0; i < unitSH.Length; i++)
        {
            unitSH[i].Init();
        }
    }
	
}
