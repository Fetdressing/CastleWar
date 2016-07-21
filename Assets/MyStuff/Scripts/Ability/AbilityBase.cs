using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AbilityBase : MonoBehaviour{
    [HideInInspector]
    public Transform thisTransform;
    [HideInInspector]
    public static TeamHandler teamHandler;

    public enum ValidTargets { Allied, Enemy, Both };
    public ValidTargets validTargets = ValidTargets.Enemy;

    [HideInInspector]
    public LayerMask enemyLayermask;
    [HideInInspector]
    public LayerMask friendlyLayermask;

    public string name;
    [HideInInspector]
    public float aoe = 0;
    public int fatigueCost = 10;
    public float cooldown = 1.0f;
    [HideInInspector]
    public float cooldown_Timer = 0.0f;

    [HideInInspector]
    public bool isInit = false;

    public virtual void InitAbility(Transform caster)
    {
        thisTransform = this.transform;

        if(teamHandler == null) //så den inte behöver hämtas flera gånger
        {
            teamHandler = GameObject.FindGameObjectWithTag("TeamHandler").GetComponent<TeamHandler>();
        }

        string[] eLayers = new string[0];
        List<string> fLayers = new List<string>();
        teamHandler.GetFriendsAndFoes(LayerMask.LayerToName(caster.gameObject.layer), ref fLayers, ref eLayers);

        for (int i = 0; i < eLayers.Length; i++)
        {
            enemyLayermask |= (1 << LayerMask.NameToLayer(eLayers[i]));
        }

        for (int i = 0; i < fLayers.Count; i++)
        {
            friendlyLayermask |= (1 << LayerMask.NameToLayer(fLayers[i]));
        }

        isInit = true;
    }

    public virtual void ApplyEffect()
    {
        if (isInit == false) return;
    }

    public virtual Transform[] ScanTargets(float aD)
    {
        LayerMask targetLayerMask;
        if(validTargets == ValidTargets.Enemy)
        {
            targetLayerMask = enemyLayermask;
        }
        else if (validTargets == ValidTargets.Allied)
        {
            targetLayerMask = friendlyLayermask;
        }
        else
        {
            targetLayerMask = teamHandler.layermaskAllTeams;
        }
        Collider[] hitColliders = Physics.OverlapSphere(thisTransform.position, aD, targetLayerMask);
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
            SortTransformsByDistance(ref hits); //index 0 kommer hamna närmst

            return hits;
        }
        else return null;
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

}
