using UnityEngine;
using System.Collections;

public class AgentStats : MonoBehaviour {
    public int level = 1;
    public int baseExpForLvl = 500; //hur mycket exp som krävs i lvl 1 för att lvla
    private int currExp = 0;

    public int startStrength = 10;
    public int startSpeed = 10;
    public int startWisdom = 10;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    if(currExp >= (baseExpForLvl * level)) //kör bara denna när man faktiskt får exp
        {
            //ding!!
        }
	}
}
