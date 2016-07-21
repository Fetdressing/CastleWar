using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AbilityManager : MonoBehaviour {
    [SerializeField]
    public List<GameObject> passiveAbilities = new List<GameObject>();
    [SerializeField]
    public List<GameObject> onHitAbilities = new List<GameObject>();
    [SerializeField]
    public List<GameObject> onAttackAbilities = new List<GameObject>();
    [SerializeField]
    public List<GameObject> castAbilities = new List<GameObject>();

    [HideInInspector]
    public List<GameObject> allAbilities = new List<GameObject>();

    public void Init()
    {
        for (int i = 0; i < passiveAbilities.Count; i++)
        {
            allAbilities.Add(passiveAbilities[i]);
        }
        for (int i = 0; i < onHitAbilities.Count; i++)
        {
            allAbilities.Add(onHitAbilities[i]);
        }
        for (int i = 0; i < onAttackAbilities.Count; i++)
        {
            allAbilities.Add(onAttackAbilities[i]);
        }
        for (int i = 0; i < castAbilities.Count; i++)
        {
            allAbilities.Add(castAbilities[i]);
        }
    }

    public int GetAbilityIndex(string abilityName)
    {
      
        for(int i = 0; i < allAbilities.Count; i++)
        {
            if(allAbilities[i].GetComponent<AbilityBase>().name == abilityName)
            {
                return i;
            }
        }
        return -100000;
    }

    public void PerformAbility(Vector3 initPoint, Transform target, int abilityIndex, float value, float aoe, LayerMask targetLayerMask) //cast range får ligga i själva agentscriptet
    {
        if(allAbilities[abilityIndex] is PassiveAbility)
        {
            GameObject ability = passiveAbilities[abilityIndex];
        }
        else if (allAbilities[abilityIndex] is OnHitAbility)
        {
            GameObject ability = onHitAbilities[abilityIndex];
        }
        else if (allAbilities[abilityIndex] is OnAttackAbility)
        {
            GameObject ability = onAttackAbilities[abilityIndex];
        }
        else if (allAbilities[abilityIndex] is CastAbility)
        {
            GameObject ability = castAbilities[abilityIndex];
        }
    }
}


//if(type is PassiveAbility)
//        {
//            for(int i = 0; i<passiveAbilities.Count; i++)
//            {
//                if(passiveAbilities[i].name == abilityName)
//                {
//                    return i;
//                }
//            }
//            return -100000;
//        }
//        else if(type is OnHitAbility)
//        {
//            for (int i = 0; i<onHitAbilities.Count; i++)
//            {
//                if (onHitAbilities[i].name == abilityName)
//                {
//                    return i;
//                }
//            }
//            return -100000;
//        }
//        else if(type is CastAbility)
//        {
//            for (int i = 0; i<castAbilities.Count; i++)
//            {
//                if (castAbilities[i].name == abilityName)
//                {
//                    return i;
//                }
//            }
//            return -100000;
//        }
//        return -100000;