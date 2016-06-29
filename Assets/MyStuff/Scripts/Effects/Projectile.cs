using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour {
    private GameObject thisObject;
    private Transform thisTransform;
    private Rigidbody thisRigidbody;
    private Transform targetE;

    private bool isReady = true; //den vill kanske inte skjuta igen förrens tex dess explosion är klar

    public bool homing = false;
    public bool friendlyFire = false;

    private int damageRoll; //skickas av skjutaren
    public float shootForce = 40;
    public float maxVelocity = 0.02f;
   
    private List<string> friendlyLayers;
    private string[] hitLayers = null;
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

    public void Dealloc()
    {
        //ta bort explosions objekt
    }
	
    public void Fire(Transform target, Vector3 aimPos, int damage, float lifeTime, bool notifyattacked, bool ff)
    {
        StopAllCoroutines();
        ToggleActive(true);
        thisTransform.LookAt(aimPos);
        targetE = target;
        thisRigidbody.AddForce(thisTransform.forward * shootForce, ForceMode.Impulse);

        notifyAttacked = notifyattacked;
        friendlyFire = ff;
        damageRoll = damage;

        startAliveTime = Time.time;
        StartCoroutine(LifeTime(lifeTime));
    }

    IEnumerator LifeTime(float time)
    {
        if(homing)
        {
            bool lostTarget = false;
            while(thisObject.activeSelf == true && (startAliveTime + time) > Time.time)
            {
                if (targetE.gameObject.activeSelf == false)
                {
                    lostTarget = true;
                }
                if (lostTarget == false)
                {
                    thisTransform.LookAt(targetE.position);
                }

                thisRigidbody.AddForce(thisTransform.forward * shootForce, ForceMode.Impulse);
                if (thisRigidbody.velocity.sqrMagnitude > maxVelocity)
                {
                    //smoothness of the slowdown is controlled by the 0.99f, 
                    //0.5f is less smooth, 0.9999f is more smooth
                    thisRigidbody.velocity *= 0.01f;
                }
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
        if (collidingUnit.transform == attackerT) //ska inte träffa en själv
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
                    if (collidingUnit.transform.GetComponent<AIBase>() != null)
                    {
                        collidingUnit.transform.GetComponent<AIBase>().Attacked(attackerT);
                    }
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
                    if (collidingUnit.transform.GetComponent<AIBase>() != null)
                    {
                        collidingUnit.transform.GetComponent<AIBase>().Attacked(attackerT);
                    }
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
                        if (collidingUnit.transform.GetComponent<AIBase>() != null)
                        {
                            collidingUnit.transform.GetComponent<AIBase>().Attacked(attackerT);
                        }
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
