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

    enum MoveState { Move, AttackMove, Patrol };
    private MoveState moveState = MoveState.Move;


    //cursor
    [Header("Cursor")]
    public CursorMode cursorMode = CursorMode.Auto;
    public Texture2D attackCursor;
    public Texture2D moveCursor;
    public Vector2 hotspot = Vector2.zero;
    //cursor

    // Use this for initialization
    void Start () {
        thisTransform = this.transform;

        //selMask = selectLayerMask.value;
    }
	
	// Update is called once per frame
	void Update () {
        SelectionBox();
        CheckTargetsAlive();

        GetMouseInput();


        if (targets.Count >= 1)
        {
            uiDisplayer.SetActive(true);
        }
        else
        {
            uiDisplayer.SetActive(false);
        }
        GetMoveState(); //behöver vara sist
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

    bool GetMousePosition(ref Vector3 pos)
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out mouseHit, Mathf.Infinity))
        {
            pos = mouseHit.point;
            return true;
        }
            return false;
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

    void CommandToPos(Vector3 pos)
    {
        switch (moveState)
        {
            case MoveState.Move:
                OrderMove(pos);
                break;

            case MoveState.AttackMove:
                OrderAttackMove(pos);
                break;
        }
    }

    void OrderMove(Vector3 pos)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            targets[i].GetComponent<AgentBase>().Move(pos);
        }
        moveState = MoveState.Move;
    }

    void OrderAttackMove(Vector3 pos)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            targets[i].GetComponent<AgentBase>().AttackMove(pos);
        }
        moveState = MoveState.Move;
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
           
                GameObject[] selectableObjects = GameObject.FindGameObjectsWithTag("Selectable");
                Rect rect = new Rect(initialSelectionBoxAnchor.x, initialSelectionBoxAnchor.y, difference.x, difference.y);

                bool foundSelection = false; //så att man ska kunna tömma den gamla arrayen om en ny selectas
                for (int i = 0; i < selectableObjects.Length; i++)
                {
                    Vector3 selPos = Camera.main.WorldToScreenPoint(selectableObjects[i].transform.position);

                    if (rect.Contains(selPos, true))
                    {
                        if (foundSelection == false)
                        {
                            if (!Input.GetButton("Add")) //men vill man däremot selecta fler så ska man få det
                            {
                                ClearTargets();
                            }
                            foundSelection = true;
                        }
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

    void GetMouseInput()
    {
        Vector3 mouseHitPos = Vector3.zero;
        Transform mouseInput = GetMouseTarget();
        bool mouseHitValid = GetMousePosition(ref mouseHitPos);

        if (Input.GetButtonDown("Fire1"))
        {
            if (moveState == MoveState.AttackMove && mouseHitValid) //specialcase på attackmove för då ska den bara attackmova
            {
                OrderAttackMove(mouseHitPos);
            }
            else
            {
                if (mouseInput != null && Input.GetButton("Add"))
                {
                    AddTarget(mouseInput);
                }
                else if (mouseInput != null)
                {
                    ClearTargets();
                    AddTarget(mouseInput);
                }
            }
        }
        else if (Input.GetButtonDown("Fire2"))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (mouseHitValid)
            {
                CommandToPos(mouseHitPos); //have target enemys aswell in here?
            }
        }

        if(moveState == MoveState.Move)
        {
            Cursor.SetCursor(moveCursor, hotspot, cursorMode);
        }
        else if(moveState == MoveState.AttackMove)
        {
            Cursor.SetCursor(attackCursor, hotspot, cursorMode);
        }
    }

    void GetMoveState()
    {
        if (Input.GetButtonDown("MoveCommand"))
        {
            moveState = MoveState.Move;
        }
        else if (Input.GetButtonDown("AttackMoveCommand"))
        {
            moveState = MoveState.AttackMove;
        }
        else if (Input.GetButtonDown("Fire1") || (Input.GetButtonDown("Cancel"))) //escape
        {
            moveState = MoveState.Move; //stänger av den igen
        }
    }
}
