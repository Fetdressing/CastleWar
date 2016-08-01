using UnityEngine;
using System.Collections;

public class ProjectilePosition : Projectile {

    public override void Fire(Transform target, Vector3 aimPos, int damage, float lifeTime, bool notifyattacked, bool ff, TypeDamage dmgType)
    {
        StopAllCoroutines();
        ToggleActive(true);
        aimPosition = aimPos;
        thisTransform.LookAt(aimPosition);
        targetE = target;
        thisRigidbody.AddForce(thisTransform.forward * shootForce, ForceMode.Impulse);

        notifyAttacked = notifyattacked;
        friendlyFire = ff;
        damageRoll = damage;
        damageType = dmgType;

        startAliveTime = Time.time;
        StartCoroutine(LifeTime(lifeTime));
    }

    IEnumerator LifeTime(float time)
    {
        while (thisObject.activeSelf == true && (startAliveTime + time) > Time.time)
        {
            if(Vector3.Distance(aimPosition, thisTransform.position) < 1.5f)
            {
                Hit();
            }
            yield return new WaitForSeconds(0.02f);
        }
        Hit();
    }

    void OnTriggerEnter(Collider collidingUnit)
    {
        if(collidingUnit.tag == "Terrain")
        {
            Hit();
        }
    }

}
