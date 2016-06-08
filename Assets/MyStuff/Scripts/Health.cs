using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Health : MonoBehaviour {

    public int startHealth = 100;
    private int maxHealth;
    private int currHealth;

    private Camera mainCamera;
    public Transform uiCanvas;
    public Image healthBar;

	// Use this for initialization
	void Start () {
        mainCamera = Camera.main;

        maxHealth = startHealth; //maxHealth kan påverkas av andra faktorer also
        currHealth = maxHealth;
	}
	
	// Update is called once per frame
	void Update ()
    {
        uiCanvas.LookAt(uiCanvas.position + mainCamera.transform.rotation * Vector3.forward,
           mainCamera.transform.rotation * Vector3.up);

	}

    public void AddHealth(int h)
    {
        currHealth += h;

        if(currHealth > maxHealth)
        {
            currHealth = maxHealth;
        }
        else if(currHealth <= 0)
        {
            //die
        }

        healthBar.fillAmount = (float)currHealth / (float)maxHealth;
    }
}
