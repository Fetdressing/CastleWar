using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class CameraMovement : MonoBehaviour {
    private Transform thisTransform;
    private Rigidbody thisRigidbody;
    private Vector3 thisPos;
    public Transform camera; //denna som ska roteras

    public float moveSpeed = 1000;
    private float threshholdValuesX;
    private float threshholdValuesY;

    private Vector3 mousePos;
	// Use this for initialization
	void Start () {
        thisTransform = this.transform;
        thisRigidbody = thisTransform.GetComponent<Rigidbody>();

        threshholdValuesX = Screen.width / 30;
        threshholdValuesY = Screen.height / 30;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        thisPos = thisTransform.position;

        mousePos = Input.mousePosition;

        if(mousePos.x < threshholdValuesX)
        {
            thisRigidbody.AddForce(Vector3.left * moveSpeed);            
        }
        else if(mousePos.x > Screen.width - threshholdValuesX)
        {
            thisRigidbody.AddForce(Vector3.right * moveSpeed);
        }


        if (mousePos.y < threshholdValuesY)
        {
            thisRigidbody.AddForce(Vector3.back * moveSpeed);
        }
        else if (mousePos.y > Screen.height - threshholdValuesY)
        {
            thisRigidbody.AddForce(Vector3.forward * moveSpeed);
        }


    }

    void Update()
    {
        //scroll
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        scrollDelta *= 15000;

        //Debug.Log(scrollDelta.ToString());

        if (scrollDelta > 0f)
        {
            //thisTransform.position = new Vector3(thisPos.x, thisPos.y - scrollDelta, thisPos.z);

            thisRigidbody.AddForce(Vector3.down * scrollDelta * Time.deltaTime, ForceMode.Impulse);
            // scroll up
        }
        else if (scrollDelta < 0f)
        {
            thisRigidbody.AddForce(Vector3.down * scrollDelta * Time.deltaTime, ForceMode.Impulse);
            //thisTransform.position = new Vector3(thisPos.x, thisPos.y + scrollDelta, thisPos.z);
            // scroll down
        }
    }

}
