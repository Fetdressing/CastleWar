using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Health : MonoBehaviour {
    private int initializedTimes = 0;
    [HideInInspector]
    public UnitSpellHandler unitSpellHandler;
    [HideInInspector]
    public AIBase aiBase;
    private Transform thisTransform;
    [HideInInspector]
    public Renderer[] thisRenderer;
    private List<Material> thisMaterial = new List<Material>();
    public float unitSize = 1;
    public Sprite unitSprite;

    [HideInInspector]
    public Vector3 middlePoint; //var dennas mittpunkt ligger
    public float middlePointOffsetY = 0.5f;

    public int startHealth = 100;
    [HideInInspector]
    public int maxHealth; //public för att den skall kunna moddas från tex AgentStats
    private int currHealth;

    public int startHealthRegAmount = 1;
    [HideInInspector]
    public int healthRegAmount;
    private float healthRegIntervall = 0.8f;
    private float healthRegTimer = 0.0f;

    public int startArmor = 3;
    [HideInInspector]
    public int armor;

    public bool isHealable = true;

    private Camera mainCamera;
    public GameObject uiHealth;
    public Image healthBar;

    public Transform uiCanvas;
    public GameObject selectionMarkerObject;

    public bool destroyOnDeath = false;
    public int resourceWorth = 5;

    //List<string> activeBuffs = new List<string>();
    Dictionary<string, Buff> buffs = new Dictionary<string, Buff>();
    // Use this for initialization
    //   void Start () {
    //       Init();
    //}

    void Awake()
    {
        //Init();
    }

    public void Init()
    {
        initializedTimes++;
        thisTransform = this.transform;
        thisRenderer = GetComponentsInChildren<Renderer>();

        if(thisTransform.GetComponent<UnitSpellHandler>() != null)
        {
            unitSpellHandler = thisTransform.GetComponent<UnitSpellHandler>();
        }

        if (thisTransform.GetComponent<AIBase>() != null)
        {
            aiBase = thisTransform.GetComponent<AIBase>();
        }
        //if (thisRenderer == null)
        //{
        //    thisRenderer = thisTransform.GetComponent<Renderer>();
        //}
        int i = 0;
        foreach (Renderer re in thisRenderer)
        {
            //Debug.Log(re.material.name);
            thisMaterial.Add(re.material);
            i++;
        }
        
        mainCamera = Camera.main;

        maxHealth = startHealth; //maxHealth kan påverkas av andra faktorer also
        currHealth = maxHealth;

        healthRegAmount = startHealthRegAmount;

        ToggleSelMarker(false);
    }
    public void Reset()
    {
        //activeBuffs.Clear();
        buffs.Clear();
        maxHealth = startHealth; //maxHealth kan påverkas av andra faktorer also
        currHealth = maxHealth;
        armor = startArmor;

        healthRegAmount = startHealthRegAmount;

        AddHealth(1); //bara så barsen ska fixas

        ToggleSelMarker(false);

        for(int i = 0; i < thisRenderer.Length; i++)
        {
            thisRenderer[i].material = thisMaterial[i];
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(initializedTimes == 0)
        {
            return;
        }
        middlePoint = new Vector3(thisTransform.position.x, thisTransform.position.y + middlePointOffsetY, thisTransform.position.z);

        uiCanvas.LookAt(uiCanvas.position + mainCamera.transform.rotation * Vector3.forward,
mainCamera.transform.rotation * Vector3.up); //vad gör jag med saker som bara har health då?

        if (currHealth >= maxHealth)
        {
            uiHealth.SetActive(false);
        }
        else
        {
            uiHealth.SetActive(true);
        }

        if (healthRegTimer < Time.time)
        {
            healthRegTimer = Time.time + healthRegIntervall;
            AddHealthTrue(healthRegAmount);
        }

    }

    public bool AddHealthTrue(int h)
    {
        if (h < 0) //ifall det är damage
        {
            if (unitSpellHandler != null)
            {
                unitSpellHandler.RegisterDamage(h);
            }

            if (h >= 0)
            {
                h = -1; //den ska ju inte heala! och minst 1 i damage
            }
        }
        else //healing
        {
            if (unitSpellHandler != null)
            {
                unitSpellHandler.RegisterHealing(h);
            }

            if (isHealable == false)
            {
                h = 0;
            }
        }

        currHealth += h;

        if (currHealth > maxHealth)
        {
            currHealth = maxHealth;
        }
        else if (currHealth <= 0)
        {
            healthBar.fillAmount = (float)currHealth / (float)maxHealth;
            Die();
            return false; //target dog
            //die
        }
        healthBar.fillAmount = (float)currHealth / (float)maxHealth;
        return true; //target vid liv
    }

    public bool AddHealth(int h)
    {
        if(h < 0) //ifall det är damage
        {
            if(unitSpellHandler != null)
            {
                unitSpellHandler.RegisterDamage(h);
            }

            h += armor; //ta bort damage med armor
            if(h >= 0)
            {
                h = -1; //den ska ju inte heala! och minst 1 i damage
            }
        }
        else //healing
        {
            if (unitSpellHandler != null)
            {
                unitSpellHandler.RegisterHealing(h);
            }

            if (isHealable == false)
            {
                h = 0;
            }
        }

        currHealth += h;
        
        if(currHealth > maxHealth)
        {
            currHealth = maxHealth;
        }
        else if(currHealth <= 0)
        {
            healthBar.fillAmount = (float)currHealth / (float)maxHealth;
            Die();
            return false; //target dog
            //die
        }
        healthBar.fillAmount = (float)currHealth / (float)maxHealth;
        return true; //target vid liv
    }

    public void Die()
    {
        GameObject.FindGameObjectWithTag("PlayerHandler").GetComponent<Builder>().ReportDeadUnit(thisTransform, resourceWorth);
        if(destroyOnDeath == true)
        {
            if (thisTransform.GetComponent<AIBase>() != null)
            {
                thisTransform.GetComponent<AIBase>().Dealloc();
            }

            Destroy(thisTransform.gameObject);
        }
        else
        {
            thisTransform.gameObject.SetActive(false);
        }        
    }

    public bool IsAlive()
    {
        if(currHealth > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public int GetCurrHealth()
    {
        return currHealth;
    }


    public void ToggleSelMarker(bool b)
    {
        selectionMarkerObject.SetActive(b);
    }

    public void ApplyMaterial(Material m, float time)
    {
        StartCoroutine(MarkMaterial(m, time));
    }

    IEnumerator MarkMaterial(Material m, float time)
    {
        //thisRenderer.material = m;
        for (int i = 0; i < thisRenderer.Length; i++)
        {
            thisRenderer[i].material = m;
        }
        yield return new WaitForSeconds(time);
        for (int i = 0; i < thisRenderer.Length; i++)
        {
            thisRenderer[i].material = thisMaterial[i];
        }
    }

    public void ApplyBuff(StatType statType, string buffName, int amount, float dur, Transform applier, bool doesStack)
    {
        string bName = applier.name + buffName;

        Buff incomingBuff = new Buff(bName, statType, amount, dur, applier, doesStack);
        if(BuffAlreadyApplied(incomingBuff))
        {
            return;
        }

        switch (statType)
        {
            case StatType.HealthReg:                
                BuffStat(ref healthRegAmount, incomingBuff);
                break;
            case StatType.Health:
                BuffStat(ref maxHealth, incomingBuff);
                break;
            case StatType.Armor:
                BuffStat(ref armor, incomingBuff);
                break;
            case StatType.Damage:
                if (aiBase == null)
                    return;
                BuffStat(ref aiBase.damage, incomingBuff);
                break;
        }

    }
    public void BuffStat(ref int stat, Buff b)
    {
        buffs.Add(b.name, b);
        stat += b.amount;
        StartCoroutine(BuffLifeTime(b.name, b));
    }

    public IEnumerator BuffLifeTime(string name , Buff b)
    {
        yield return new WaitForSeconds(b.duration);
        BuffEnded(name);
    }

    public void BuffEnded(string name)
    {
        Buff tempBuff;
        if(!buffs.TryGetValue(name, out tempBuff))
        {
            Debug.Log("LYCKADES INTE HÄMTA VÄRDET, BUFFEN FANNS EJ!!");
        }

        switch (tempBuff.statType)
        {
            case StatType.HealthReg:
                healthRegAmount += -tempBuff.amount; //inversed amount för att ta bort igen
                break;
            case StatType.Health:
                maxHealth += -tempBuff.amount;
                break;
            case StatType.Armor:
                armor += -tempBuff.amount;
                break;
            case StatType.Damage:
                aiBase.damage += -tempBuff.amount;
                break;
        }
        buffs.Remove(name);
    }

    public bool BuffAlreadyApplied(Buff b)
    {
        Buff tempB;
        if (buffs.TryGetValue(b.name, out tempB)) //finns det någon med samma namn?
        {
            return true; //fanns redan
        }
        else return false;

        //for(int i = 0; i < buffs.Count; i++)
        //{
        //    if(b == buffs[i])
        //    {
        //        return true;
        //    }
        //}
        //return false;
    }

}

public enum StatType { Health, HealthReg, Armor, Damage, DamageSpread, MovementSpeed, AttackSpeed };

public class Buff
{
    public string name;
    public StatType statType;
    public int amount;
    public float duration;
    public Transform applier;
    public bool doesStack;

    public bool hasEnded;

    public Buff(string nameC, StatType statTypeC, int amountC, float durationC, Transform applierC, bool doesStackC)
    {
        name = nameC;
        statType = statTypeC;
        amount = amountC;
        duration = durationC;
        applier = applierC;
        doesStack = doesStackC;
    }
}
