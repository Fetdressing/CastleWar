using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour {
    private GameObject thisObject;
    private Transform thisTransform;
    private Rigidbody thisRigidbody;

    private bool isReady = true; //den vill kanske inte skjuta igen förrens tex dess explosion är klar

    public bool homing = false;
    public bool friendlyFire = false;

    private int damageRoll; //skickas av skjutaren
    private float shootForce = 40;
   
    private List<string> friendlyLayers;
    private string[] hitLayers;
    private Transform attackerT;
    private bool notifyAttacked = false; //denna bestäms av den som skjuter

    private float startAliveTime = 0.0f; //när den börja leva, kolla mot hur länge den ska leva
	// Use this for initialization
	void Awake () {
        thisObject = this.gameObject;
        thisTransform = this.transform;
        thisRigidbody = thisTransform.GetComponent<Rigidbody>();
	}

    public void Init(string[] hitlayers, List<string> friendlylayers, Transform attacker) //vilka saker den ska kunna göra skada på
    {
        hitLayers = hitlayers;
        friendlyLayers = friendlylayers;
        attackerT = attacker;
        ToggleActive(false);
    }
	
    public void Fire(Transform target, int damage, int lifeTime, bool notifyattacked)
    {
        ToggleActive(true);
        thisTransform.LookAt(target.position);
        thisRigidbody.AddForce(thisTransform.forward * shootForce, ForceMode.Impulse);

        notifyAttacked = notifyattacked;
        damageRoll = damage;

        startAliveTime = Time.time;
        StartCoroutine(LifeTime(lifeTime));
    }

    IEnumerator LifeTime(int time)
    {
        if(homing)
        {
            while(thisObject.activeSelf == true && (startAliveTime + time) > Time.time)
            {

                yield return new WaitForSeconds(0.1f);
            }
            ToggleActive(false);
        }
        else
        {
            yield return new WaitForSeconds(time);
            ToggleActive(false);
        }
    }

    void OnTriggerEnter(Collider collidingUnit)
    {
        if (collidingUnit.transform == attackerT)
        {
            return;
        }

        for (int i = 0; i < hitLayers.Length; i++)
        {
            if(collidingUnit.gameObject.layer == LayerMask.NameToLayer(hitLayers[i]))
            {
                //deal damage och skicka attackerna till AgentBase
                Hit();
                collidingUnit.GetComponent<Health>().AddHealth(-damageRoll);
                if(notifyAttacked)
                {
                    collidingUnit.transform.GetComponent<AgentBase>().Attacked(attackerT);
                }
                return;
            }
        }

        if(collidingUnit.gameObject.layer == attackerT.gameObject.layer) //attackera en från samma team
        {
            if (friendlyFire == true)
            {
                collidingUnit.GetComponent<Health>().AddHealth(-damageRoll);
                if (notifyAttacked)
                {
                    collidingUnit.transform.GetComponent<AgentBase>().Attacked(attackerT);
                }
            }
            return;
        }

        for (int i = 0; i < friendlyLayers.Count; i++) //så man inte träffar sig själv
        {
            if (collidingUnit.gameObject.layer == LayerMask.NameToLayer(friendlyLayers[i]))
            {
                if (friendlyFire == true)
                {
                    collidingUnit.GetComponent<Health>().AddHealth(-damageRoll);
                    if (notifyAttacked)
                    {
                        collidingUnit.transform.GetComponent<AgentBase>().Attacked(attackerT);
                    }
                }
                return;
            }
        }

        Hit(); //den träffade terräng annars
    }

    public void Hit() //explosion
    {

        ToggleActive(false);
    }

    public void ToggleActive(bool b)
    {
        if(b)
        {
            thisObject.SetActive(true);
        }
        else
        {
            thisRigidbody.velocity = new Vector3(0,0,0);
            thisObject.SetActive(false);
        }
    }

    public bool IsReady()
    {
        if(thisObject.activeSelf == false)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
