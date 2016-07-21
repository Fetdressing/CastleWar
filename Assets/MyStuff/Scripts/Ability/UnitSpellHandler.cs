using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitSpellHandler : MonoBehaviour {
    private Transform thisTransform;
    private AgentBase agentBase;
    private Health health;
    private static AbilityManager abilityManager;

    public string[] abilityNames;
    [HideInInspector]
    public List<int> abilityIndexes = new List<int>();

    private int attackCounter = 0; //hur många attacker denna har utfört, för att applya onhit

    [HideInInspector]
    public List<PassiveAbility> passiveAbilities = new List<PassiveAbility>();
    [HideInInspector]
    public List<OnHitAbility> onHitAbilities = new List<OnHitAbility>();
    [HideInInspector]
    public List<OnAttackAbility> onAttackAbilities = new List<OnAttackAbility>();
    [HideInInspector]
    public List<CastAbility> castAbilities = new List<CastAbility>();

    //vart de skall utgå ifrån, fäst spellen vid deras position
    public Transform auraPosition;
    public Transform onHitPosition;
    public Transform onAttackPosition;
    public Transform castPosition;

    [HideInInspector]
    public bool isCasting = false;
    void Start () {
        Init();
	}
	
    public void Init()
    {
        if (abilityManager == null)
        {
            abilityManager = GameObject.FindGameObjectWithTag("AbilityManager").GetComponent<AbilityManager>();
        }

        for (int i = 0; i < abilityNames.Length; i++)
        {
            abilityIndexes.Add(abilityManager.GetAbilityIndex(abilityNames[i]));
        }

        for(int i = 0; i < abilityIndexes.Count; i++)
        {
            InitAbility(i); //kan ju inte bara init denna coz det är ett kinda abstrakt värde, init ska ske på denna unitspellhandlern själv
        }
        Reset();
    }

    public void Reset()
    {
        isCasting = false;
    }

    public void InitAbility(int index)
    {
        GameObject temp = Instantiate(abilityManager.allAbilities[index], thisTransform.position, Quaternion.identity) as GameObject; //instantiera spellen, sen sköter den resten
        temp.transform.SetParent(thisTransform);

        AbilityBase tempType = temp.GetComponent<AbilityBase>();
        
        //sortera dem
        if (tempType is PassiveAbility)
        {
            passiveAbilities.Add(temp.GetComponent<PassiveAbility>());
        }
        else if (tempType is OnHitAbility)
        {
            onHitAbilities.Add(temp.GetComponent<OnHitAbility>());
        }
        else if (tempType is OnAttackAbility)
        {
            onAttackAbilities.Add(temp.GetComponent<OnAttackAbility>());
        }
        else if (tempType is CastAbility)
        {
            castAbilities.Add(temp.GetComponent<CastAbility>());
        }
    }

    public void CastSpell() //man får kanske skicka in ett index
    {

    }

    public void RegisterAttack()
    {
        attackCounter++;
        for(int i = 0; i < onAttackAbilities.Count; i++)
        {
            onAttackAbilities[i].ApplyEffect();
        }
    }

    public void RegisterDamage(int damage) //tänk på att damage är negativ
    {
        for (int i = 0; i < onHitAbilities.Count; i++)
        {
            onHitAbilities[i].ApplyEffect();
        }
    }

    public void RegisterHealing(int healing)
    {
        for (int i = 0; i < onHitAbilities.Count; i++)
        {
            onHitAbilities[i].ApplyEffect();
        }
    }
}
