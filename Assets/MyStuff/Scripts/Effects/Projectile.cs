using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour {
    [HideInInspector] public int initTimes = 0;

    [HideInInspector] public GameObject thisObject;
    [HideInInspector] public Transform thisTransform;
    [HideInInspector] public Rigidbody thisRigidbody;
    [HideInInspector] public Transform targetE;
    [HideInInspector]
    public Vector3 aimPosition;

    [HideInInspector] public bool isReady = true; //den vill kanske inte skjuta igen förrens tex dess explosion är klar

    public bool homing = false;
    public bool friendlyFire = false;

    [HideInInspector] public int damageRoll; //skickas av skjutaren
    public float shootForce = 40;
    public float maxVelocity = 0.02f;
    public float aoeRange = 0;
   
    [HideInInspector] public List<string> friendlyLayers;
    [HideInInspector] public string[] hitLayers = null;
    [HideInInspector]
    public LayerMask friendlyLM;
    [HideInInspector]
    public LayerMask enemyLM;
    [HideInInspector]
    public LayerMask allLM;

    [HideInInspector] public Transform attackerT;
    [HideInInspector] public bool notifyAttacked = false; //denna bestäms av den som skjuter

    [HideInInspector] public float startAliveTime = 0.0f; //när den börja leva, kolla mot hur länge den ska leva

    public GameObject explosionObj;
    [HideInInspector]
    public List<GameObject> explosionPool = new List<GameObject>();
    public int explosionPoolSize = 2;
    // Use this for initialization
    void Awake () {
        thisObject = this.gameObject;
        thisTransform = this.transform;
        thisRigidbody = thisTransform.GetComponent<Rigidbody>();
	}

    public void Init(string[] hitlayers, List<string> friendlylayers, Transform attacker) //vilka saker den ska kunna göra skada på
    {
        if (initTimes != 0) return;
        initTimes++;
        hitLayers = hitlayers;
        friendlyLayers = friendlylayers;
        attackerT = attacker;
        ToggleActive(false);

        InitLayermasks();
        InitObjectPool();
    }

    public void InitLayermasks()
    {
        enemyLM = LayerMask.NameToLayer("Nothing"); //inte riktigt säker på varför detta funkar men det gör
        friendlyLM = LayerMask.NameToLayer("Nothing");
        allLM = LayerMask.NameToLayer("Nothing");

        enemyLM = ~enemyLM; //inte riktigt säker på varför detta funkar men det gör
        friendlyLM = ~friendlyLM;
        allLM = ~allLM;

        for (int i = 0; i < hitLayers.Length; i++)
        {
            enemyLM |= (1 << LayerMask.NameToLayer(hitLayers[i]));
            allLM |= (1 << LayerMask.NameToLayer(hitLayers[i]));
        }
        for(int i = 0; i < friendlyLayers.Count; i++)
        {
            friendlyLM |= (1 << LayerMask.NameToLayer(friendlyLayers[i]));
            allLM |= (1 << LayerMask.NameToLayer(friendlyLayers[i]));
        }
    }

    public void InitObjectPool()
    {
        if (explosionObj == null) return;
        for (int i = 0; i < explosionPoolSize; i++)
        {
            GameObject tempO = Instantiate(explosionObj.gameObject) as GameObject;
            explosionPool.Add(tempO.gameObject);
            tempO.SetActive(false);
        }
    }

    public void Dealloc()
    {
        StopAllCoroutines();
        //ta bort explosions objekt
        if (explosionObj == null) return;
        for (int i = 0; i < explosionPoolSize; i++)
        {
            Destroy(explosionPool[i].gameObject);
        }
    }
	
    public virtual void Fire(Transform target, Vector3 aimPos, int damage, float lifeTime, bool notifyattacked, bool ff)
    {
        StopAllCoroutines();
        ToggleActive(true);
        thisTransform.LookAt(aimPos);
        aimPosition = aimPos;
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
            while(attackerT != null && thisObject.activeSelf == true && (startAliveTime + time) > Time.time)
            {
                if (targetE == null || targetE.gameObject.activeSelf == false)
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
        //Debug.Log(Time.time.ToString());
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
                ApplyDamageTransform(collidingUnit.transform);
                return;
            }
        }

        //if(collidingUnit.gameObject.layer == attackerT.gameObject.layer) //attackera en från samma team
        //{
        //    if (friendlyFire == true && collidingUnit.transform == targetE) //om man attackerar allierad så ska den bara träffa just de targetet
        //    {
        //        ApplyDamageTransform(collidingUnit.transform);
        //    }
        //    return;
        //}

        for (int i = 0; i < friendlyLayers.Count; i++) //attackerar en allierad
        {
            if (collidingUnit.gameObject.layer == LayerMask.NameToLayer(friendlyLayers[i]))
            {
                if (friendlyFire == true && collidingUnit.transform == targetE) //om man attackerar allierad så ska den bara träffa just de targetet
                {
                    Hit();
                    ApplyDamageTransform(collidingUnit.transform);
                }
                return;
            }
        }

        Hit(); //den träffade terräng annars
    }

    public virtual void Hit() //explosion
    {
        DealAOEDamage();

        if (explosionObj != null)
        {
            GameObject readyExplosion = null;
            for (int i = 0; i < explosionPool.Count; i++)
            {
                if (explosionPool[i].activeSelf == false)
                {
                    readyExplosion = explosionPool[i].gameObject;
                    break;
                }
            }
            if (readyExplosion != null)
            {
                readyExplosion.transform.position = thisTransform.position;
                readyExplosion.GetComponent<ParticleTimed>().StartParticleSystem();
            }
        }

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

    public void ApplyDamageTransform(Transform t)
    {
        t.GetComponent<Health>().AddHealth(-damageRoll);
        if (notifyAttacked)
        {
            if (t.transform.GetComponent<AIBase>() != null)
            {
                t.transform.GetComponent<AIBase>().Attacked(attackerT);
            }
        }
    }

    public void DealAOEDamage()
    {
        if (aoeRange < 0.5f) return;

        LayerMask targetLM;
        if(friendlyFire == true)
        {
            targetLM = allLM;
        }
        else
        {
            targetLM = enemyLM;
        }

        Collider[] hitColliders = Physics.OverlapSphere(thisTransform.position, aoeRange, targetLM);
        //int i = 0;
        //while (i < hitColliders.Length)
        //{
        //    Debug.Log(hitColliders[i].transform.name);
        //    i++;
        //}

        if (hitColliders.Length > 0)
        {
            for (int i = 0; i < hitColliders.Length; i++)
            {
                ApplyDamageTransform(hitColliders[i].transform);
            }
            //SortTransformsByDistance(ref hits); //index 0 kommer hamna närmst

            
        }
    }
}
