using UnityEngine;
using System.Collections;

public class AgentRanged : AgentBase {

    [Header("Stats")]
    public GameObject projectile;

    // Use this for initialization
    void Start () {
        Init();
	}
	
	// Update is called once per frame
	void Update () {
        uiCanvas.LookAt(uiCanvas.position + mainCamera.transform.rotation * Vector3.forward,
  mainCamera.transform.rotation * Vector3.up);

        if (target != null)
        {
            targetDistance = GetTargetDistance();
        }

        switch (state)
        {
            case UnitState.Guarding:
                GuardingUpdate();
                break;


            case UnitState.AttackMoving:
                AttackMovingUpdate();
                break;

            case UnitState.Moving: //nått som kollar ifall jag kommit fram och isåfall vill jag nog vakta
                MovingUpdate();
                break;
        }
    }

    public override void Init()
    {
        base.Init();
    }

    public override void AttackTarget()
    {
           
        if (attackRange > targetDistance) //kolla så att target står framför mig oxå
        {
            if (attackSpeedTimer <= Time.time)
            {
                attackSpeedTimer = attackSpeed + Time.time;
                int damageRoll = Random.Range(damageMIN, damageMAX);
                target.GetComponent<Health>().AddHealth(-damageRoll);
                //damage target!
            }
        }
        if (attackRange + 1 < targetDistance) //+1 för marginal
        {
            agent.SetDestination(target.position);
        }    
    }
}
