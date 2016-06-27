using UnityEngine;
using System.Collections;

public class FadingLight : MonoBehaviour {
    private Light light;

    public float fadeTime = 4;
    public float fadeTick = 0.1f;
    private float startIntensity;
	// Use this for initialization
	void Awake () {
        light = this.transform.GetComponent<Light>();
        startIntensity = light.intensity;

        //StartFade();
	}
	
	// Update is called once per frame

    public void StartFade()
    {
        this.gameObject.SetActive(true);
        StartCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        float timer = Time.time + fadeTime;
        light.intensity = startIntensity;

        while (timer > Time.time)
        {
            light.intensity -= fadeTick;
            yield return new WaitForSeconds(0.05f);
        }
        this.gameObject.SetActive(false);
    }
}
