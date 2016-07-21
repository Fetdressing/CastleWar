using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Health : MonoBehaviour {
    private int initializedTimes = 0;
    [HideInInspector]
    public UnitSpellHandler unitSpellHandler;
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

    public int armor = 3;

    public bool isHealable = true;

    private Camera mainCamera;
    public GameObject uiHealth;
    public Image healthBar;

    public Transform uiCanvas;
    public GameObject selectionMarkerObject;

    public bool destroyOnDeath = false;
    public int resourceWorth = 5;

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
        maxHealth = startHealth; //maxHealth kan påverkas av andra faktorer also
        currHealth = maxHealth;

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

            if(healthRegTimer < Time.time)
            {
                healthRegTimer = Time.time + healthRegIntervall;
                AddHealth(healthRegAmount);
            }
        }

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
}
