using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitSpellHandler : MonoBehaviour {
    private Transform thisTransform;
    private AIBase AIBase;
    private Health health;
    private static AbilityManager abilityManager;

    public string[] abilityNames; //de kommer i denna ordningen antar jag
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
    [HideInInspector]
    public List<AbilityBase> allAbilities = new List<AbilityBase>();

    //vart de skall utgå ifrån, fäst spellen vid deras position
    public Transform auraPosition;
    public Transform onHitPosition;
    public Transform onAttackPosition;
    public Transform castPosition;

    [HideInInspector]
    public bool isCasting = false;

    public float intervalTime = 0.5f;

    bool isInit = false;
    void Start () {
        //Init();
	}
	
    public void Init()
    {
        if (isInit == true)
            return;
        isInit = true;

        thisTransform = this.transform;
        AIBase = thisTransform.GetComponent<AIBase>();
        health = thisTransform.GetComponent<Health>();
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
            InitAbility(abilityIndexes[i]); //kan ju inte bara init denna coz det är ett kinda abstrakt värde, init ska ske på denna unitspellhandlern själv
        }
        LoadAbilitySprites();
        Reset();
    }

    public void LoadAbilitySprites()
    {
        for(int i = 0; i < allAbilities.Count; i++)
        {
            //allAbilities[i].abilitySprite;
        }
        //abilityManager.GetAbilitySprite(1);
        Debug.Log("FIXA MED SPRITES!!");
    }

    public void Reset()
    {
        StopAllCoroutines();
        isCasting = false;
        StartCoroutine(AbilityInterval());
    }

    public void InitAbility(int index)
    {
        if (index < 0) return;
        //Debug.Log(abilityManager.allAbilities[0].gameObject.name);
        GameObject temp = Instantiate(abilityManager.allAbilities[index].gameObject, thisTransform.position, Quaternion.identity) as GameObject; //instantiera spellen, sen sköter den resten
        temp.transform.SetParent(thisTransform);

        AbilityBase tempType = temp.GetComponent<AbilityBase>();
        
        //sortera dem
        if (tempType is PassiveAbility)
        {
            passiveAbilities.Add(temp.GetComponent<PassiveAbility>());
            temp.transform.position = auraPosition.position;
        }
        else if (tempType is OnHitAbility)
        {
            onHitAbilities.Add(temp.GetComponent<OnHitAbility>());
            temp.transform.position = onHitPosition.position;
        }
        else if (tempType is OnAttackAbility)
        {
            onAttackAbilities.Add(temp.GetComponent<OnAttackAbility>());
            temp.transform.position = onAttackPosition.position;
        }
        else if (tempType is CastAbility)
        {
            castAbilities.Add(temp.GetComponent<CastAbility>());
            temp.transform.position = castPosition.position;
        }
        allAbilities.Add(tempType); //lägg till den i den sammansatta listan
        tempType.InitAbility(thisTransform);
    }

    IEnumerator AbilityInterval()
    {
        while(this != null)
        {
            for(int i = 0; i < passiveAbilities.Count; i++)
            {
                passiveAbilities[i].ApplyEffect();
            }
            yield return new WaitForSeconds(intervalTime);
        }
    }

    public bool CastSpell(Vector3 pos, int spellIndex, ref bool isCastable, int currFatigue) //man får kanske skicka in ett index
    { //indexet gäller alla abilities då den ska visa upp alla
        if (allAbilities[spellIndex].GetComponent<CastAbility>() == null) //kolla så att det är en spell som går att kasta
        {
            isCastable = false;
            return false;
        }
        int spellCost = allAbilities[spellIndex].GetComponent<CastAbility>().CastSpell(pos, currFatigue, ref isCastable); //returnerar 0 ifall det inte gick
        if (spellCost == 0) //gick inte kasta
        {
            return false;
        }
        else
        {
            //currFatigue -= costFat;
            return true;
        }
    }

    public bool IsSpellReady(int spellIndex, int currFatigue)
    {
        if(!SpellIndexExists(spellIndex))
        {
            return false;
        }
        return (allAbilities[spellIndex].IsReady(currFatigue));
    }

    public bool SpellIndexExists(int spellIndex) //ifall det finns en spell på denna plats
    {
        if(spellIndex >= allAbilities.Count)
        {
            return false;
        }

        if(allAbilities[spellIndex] == null)
        {
            return false;
        }
        return true;
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

    void OnDestroy()
    {
        for(int i = 0; i < allAbilities.Count; i++)
        {
            allAbilities[i].Dealloc(); //så pools o stuff tas bort
        }
    }
}
