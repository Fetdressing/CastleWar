using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TeamHandler : MonoBehaviour {
    public Team[] teams;

    // Use this for initialization
    void Start () {
	
	}

    public bool GetFriendsAndFoes(string layer, ref List<string> friendlys, ref string[] enemies)
    {
        bool success = false;
        Team currTeam = new Team();
        for(int i = 0; i < teams.Length; i++)
        {
            if(layer == teams[i].thisLayer)
            {
                success = true;
                currTeam = teams[i];
                break;
            }
        }

        friendlys.Clear();
        for(int i = 0; i < currTeam.friendlyTeams.Length; i++)
        {
            friendlys.Add(currTeam.friendlyTeams[i]);
        }
        enemies = currTeam.enemyTeams;

        return success;
    }

    [System.Serializable]
    public struct Team
    {
        public string thisLayer;
        public string[] friendlyTeams;
        public string[] enemyTeams;

    }
}
