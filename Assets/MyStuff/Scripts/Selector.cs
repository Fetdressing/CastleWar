using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Selector : MonoBehaviour {
    private Transform thisTransform;

    private RaycastHit mouseHit;
    public LayerMask selectLayerMask = -1;
    //private int selMask;

    public GameObject uiDisplayer;

    private List<Transform> targets = new List<Transform>();

    //selectionBox
    public GameObject selectionCanvas;
    public RectTransform selectionBox;
    private Vector2 initialSelectionBoxAnchor = Vector2.zero;
    //selectionBox

	// Use this for initialization
	void Start () {
        thisTransform = this.transform;

        //selMask = selectLayerMask.value;
    }
	
	// Update is called once per frame
	void Update () {
        SelectionBox();
        CheckTargetsAlive();

        Transform mouseInput = GetMouseTarget();

        if(Input.GetButtonDown("Fire1"))
        {
            if (mouseInput != null && Input.GetButton("Add"))
            {
                AddTarget(mouseInput);
            }
            else if(mouseInput != null)
            {
                ClearTargets();
                //targets.Add(mouseInput);
                AddTarget(mouseInput);
            }
            //else
            //{
            //    targets.Clear();
            //}
        }
        else if(Input.GetButtonDown("Fire2"))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out mouseHit, Mathf.Infinity))
            {
                OrderMove(mouseHit.point);
            }
        }

        
        if (mouseInput != null || targets.Count >= 1)
        {
            uiDisplayer.SetActive(true);
        }
        else
        {
            uiDisplayer.SetActive(false);
        }
    }

    Transform GetMouseTarget()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out mouseHit, Mathf.Infinity, selectLayerMask))
        {
            if (mouseHit.collider != null)
            {
                return mouseHit.collider.transform;
            }
        }

        return null;
    }

    void CheckTargetsAlive()
    {
        for(int i = 0; i < targets.Count; i++)
        {
            if(targets[i] == null || targets[i].gameObject.activeSelf == false)
            {
                RemoveTarget(i);
            }
        }
    }

    void AddTarget(Transform newTarget)
    {
        bool exists = false; //kolla så att den inte redan är med i listan
        for (int i = 0; i < targets.Count; i++)
        {
            //Debug.Log(targets[i].ToString());
            if(newTarget == targets[i])
            {
                exists = true;
            }
        }

        if (exists == false)
        {
            newTarget.GetComponent<AgentBase>().ToggleSelMarker(true);
            targets.Add(newTarget);
        }
    }

    void RemoveTarget(int i) //removes target by index
    {
        if (targets[i] != null)
        {
            targets[i].GetComponent<AgentBase>().ToggleSelMarker(false);
        }
        targets.RemoveAt(i);
    }

    void ClearTargets()
    {
        for(int i = 0; i < targets.Count; i++)
        {
            targets[i].GetComponent<AgentBase>().ToggleSelMarker(false);            
        }
        targets.Clear();
    }

    void OrderMove(Vector3 pos)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            targets[i].GetComponent<AgentBase>().Move(pos);
        }
    }

    void SelectionBox()
    {
        // Click somewhere in the Game View.
        if (Input.GetMouseButtonDown(0))
        {
            // Get the initial click position of the mouse. No need to convert to GUI space
            // since we are using the lower left as anchor and pivot.
            initialSelectionBoxAnchor = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

            // The anchor is set to the same place.
            selectionBox.anchoredPosition = initialSelectionBoxAnchor;
        }
        // Store the current mouse position in screen space.
        Vector2 currentMousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        // How far have we moved the mouse?
        Vector2 difference = currentMousePosition - initialSelectionBoxAnchor;
        // While we are dragging.
        if (Input.GetMouseButton(0))
        {
            // Copy the initial click position to a new variable. Using the original variable will cause
            // the anchor to move around to wherever the current mouse position is,
            // which isn't desirable.
            Vector2 startPoint = initialSelectionBoxAnchor;

            // The following code accounts for dragging in various directions.
            if (difference.x < 0)
            {
                startPoint.x = currentMousePosition.x;
                difference.x = -difference.x;
            }
            if (difference.y < 0)
            {
                startPoint.y = currentMousePosition.y;
                difference.y = -difference.y;
            }

            // Set the anchor, width and height every frame.
            selectionBox.anchoredPosition = startPoint;
            selectionBox.sizeDelta = difference;
        }

        // After we release the mouse button.
        if (Input.GetMouseButtonUp(0))
        {
            // Reset
            if (Mathf.Abs(difference.x) > Screen.width/100 || Mathf.Abs(difference.y) > Screen.height / 100) //så att ett simpelt klick inte ska avmarkera den vanliga
            {
                ClearTargets();
                GameObject[] selectableObjects = GameObject.FindGameObjectsWithTag("Selectable");
                Rect rect = new Rect(initialSelectionBoxAnchor.x, initialSelectionBoxAnchor.y, difference.x, difference.y);
                for (int i = 0; i < selectableObjects.Length; i++)
                {
                    Vector3 selPos = Camera.main.WorldToScreenPoint(selectableObjects[i].transform.position);
                    //selPos = new Vector3(selPos.x * Screen.width, selPos.y * Screen.height, selPos.z);
                    //Debug.Log(difference.ToString());

                    if (rect.Contains(selPos, true))
                    {
                        //targets.Add(selectableObjects[i].transform);
                        AddTarget(selectableObjects[i].transform);
                    }
                }
            }

            initialSelectionBoxAnchor = Vector2.zero;
            selectionBox.anchoredPosition = Vector2.zero;
            selectionBox.sizeDelta = Vector2.zero;
            
        }
    }
}
