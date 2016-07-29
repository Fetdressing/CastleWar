using UnityEngine;
using System.Collections;

public class ParticleTimed : MonoBehaviour {
    private GameObject thisObject;
    public int lifeTime = 3;
	// Use this for initialization
	void Start () {
        thisObject = this.gameObject;
	}
    void Awake()
    {
        thisObject = this.gameObject;
    }
	
	public void StartParticleSystem()
    {
        thisObject.SetActive(true);
        StartCoroutine(RunParticleSystem());
    }
    IEnumerator RunParticleSystem()
    {
        yield return new WaitForSeconds(lifeTime);
        thisObject.SetActive(false);
    }
}
