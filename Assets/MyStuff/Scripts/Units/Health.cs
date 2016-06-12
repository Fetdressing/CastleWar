using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Health : MonoBehaviour {
    [HideInInspector]
    public bool isAlive = true;

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

	// Use this for initialization
	void Start () {
        isAlive = true;
        mainCamera = Camera.main;

        maxHealth = startHealth; //maxHealth kan påverkas av andra faktorer also
        currHealth = maxHealth;

        healthRegAmount = startHealthRegAmount;
	}
	
	// Update is called once per frame
	void Update ()
    {

        if(currHealth >= maxHealth)
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
            isAlive = false;
            Destroy(this.gameObject);
            return false; //target dog
            //die
        }
        healthBar.fillAmount = (float)currHealth / (float)maxHealth;
        return true; //target vid liv
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
}
