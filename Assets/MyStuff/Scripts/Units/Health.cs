using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Health : MonoBehaviour {
    private Transform thisTransform;
    public float unitSize = 1;

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
    public float healthRegIntervall = 1.5f;
    private float healthRegTimer = 0.0f;

    public int armor = 3;

    public bool isHealable = true;

    private Camera mainCamera;
    public GameObject uiHealth;
    public Image healthBar;

    public Transform uiCanvas;
    public GameObject selectionMarkerObject;

    public bool destroyOnDeath = false;

    // Use this for initialization
 //   void Start () {
 //       Init();
	//}

    void Awake()
    {
        Init();
    }

    public void Init()
    {
        thisTransform = this.transform;
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
    }
	
	// Update is called once per frame
	void Update ()
    {
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
            h += armor; //ta bort damage med armor
            if(h >= 0)
            {
                h = -1; //den ska ju inte heala! och minst 1 i damage
            }
        }
        else //healing
        {
            if(isHealable == false)
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
}
