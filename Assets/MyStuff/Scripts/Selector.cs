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

    enum MoveState { Move, Attack, Patrol };
    private MoveState moveState = MoveState.Move;

    public float movePositionsMult = 3; // hur långt ifrån de ska ställa sig från varandra

    //cursor
    [Header("Cursor")]
    public CursorMode cursorMode = CursorMode.Auto;
    public Texture2D attackCursor;
    public Texture2D moveCursor;
    public Vector2 hotspot = Vector2.zero;
    //cursor

    public GameObject groundMarkerObject;
    private List<Transform> groundMarkerPool = new List<Transform>();
    private int groundMarkerPoolSize = 7;
    private int roundRobinIndex = 0; //för groundmarkern

    // Use this for initialization
    void Start () {
        thisTransform = this.transform;

        Cursor.SetCursor(moveCursor, hotspot, cursorMode);

        for(int i = 0; i < groundMarkerPoolSize; i++)
        {
            GameObject temp = GameObject.Instantiate(groundMarkerObject.gameObject);
            temp.SetActive(false);
            groundMarkerPool.Add(temp.transform);
        }
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
            newTarget.GetComponent<Health>().ToggleSelMarker(true);
            targets.Add(newTarget);
        }
    }

    void RemoveTarget(int i) //removes target by index
    {
        if (targets[i] != null)
        {
            targets[i].GetComponent<Health>().ToggleSelMarker(false);
        }
        targets.RemoveAt(i);
    }

    void ClearTargets()
    {
        for(int i = 0; i < targets.Count; i++)
        {
            targets[i].GetComponent<Health>().ToggleSelMarker(false);            
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

            case MoveState.Attack:
                OrderAttackMove(pos);
                break;
        }
    }

    void OrderMove(Vector3 pos)
    {
        PlaceGroundMarker(pos + new Vector3(0, 0.2f, 0));
        if (moveState == MoveState.Move) //annars ska den bara avmarkera den andra statet, som typ attack
        {
            List<Vector3> movePositions = new List<Vector3>();
            if(targets.Count > 2) //använd bara box pattern när det är fler än 2
            {
                movePositions = GetBoxPattern(targets, pos);
            }
            else
            {
                for(int i = 0; i < targets.Count; i++)
                {
                    movePositions.Add(pos);
                }
            }

            if (Input.GetButton("Add")) //om add så lägg till kommandot i listan
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i].GetComponent<AIBase>() != null)
                    {
                        targets[i].GetComponent<AIBase>().AddCommandToList(movePositions[i], AIBase.UnitState.Moving, null, false);
                    }
                }
            }
            else
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i].GetComponent<AIBase>() != null)
                    {
                        targets[i].GetComponent<AIBase>().ClearCommands();
                        targets[i].GetComponent<AIBase>().Move(movePositions[i]);
                    }
                }
            }
        }
        moveState = MoveState.Move;
        Cursor.SetCursor(moveCursor, hotspot, cursorMode);
    }

    void OrderAttackMove(Vector3 pos)
    {
        PlaceGroundMarker(pos + new Vector3(0, 0.2f, 0));

        List<Vector3> movePositions = new List<Vector3>();
        if (targets.Count > 2) //använd bara box pattern när det är fler än 2
        {
            movePositions = GetBoxPattern(targets, pos);
        }
        else
        {
            for (int i = 0; i < targets.Count; i++)
            {
                movePositions.Add(pos);
            }
        }

        if (Input.GetButton("Add"))
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i].GetComponent<AIBase>() != null)
                {
                    targets[i].GetComponent<AIBase>().AddCommandToList(movePositions[i], AIBase.UnitState.AttackMoving, null, false);
                }
            }
        }
        else
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i].GetComponent<AIBase>() != null)
                {
                    targets[i].GetComponent<AIBase>().ClearCommands();
                    targets[i].GetComponent<AIBase>().AttackMove(movePositions[i]);
                }
            }
        }

        moveState = MoveState.Move;
        Cursor.SetCursor(moveCursor, hotspot, cursorMode);
    }

    void OrderAttackUnit(Transform t, bool friendfire)
    {
        PlaceGroundMarker(t.position + new Vector3(0, 0.2f, 0));
        if (Input.GetButton("Add"))
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i].GetComponent<AIBase>() != null)
                {
                    targets[i].GetComponent<AIBase>().AddCommandToList(t.position, AIBase.UnitState.AttackingUnit, t, friendfire);
                }
            }
        }
        else
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i].GetComponent<AIBase>() != null)
                {
                    targets[i].GetComponent<AIBase>().ClearCommands();
                    targets[i].GetComponent<AIBase>().AttackUnit(t, friendfire);
                }
            }
        }
        moveState = MoveState.Move;
        Cursor.SetCursor(moveCursor, hotspot, cursorMode);
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

        if (Input.GetButtonDown("Fire1")) //attack unit om man har MoveState.Attack och ifall mouseInput != null!!!!!!!!!!!!!!!!!!!!!!!
        {
            if (moveState == MoveState.Attack && mouseInput != null)
            {
                OrderAttackUnit(mouseInput, true);
            }
            else if (moveState == MoveState.Attack && mouseHitValid) //specialcase på attackmove för då ska den bara attackmova
            {
                OrderAttackMove(mouseHitPos);
            }
            else
            {
                if (mouseInput != null && Input.GetButton("Add")) //kolla oxå så att det inte bara är en destructable
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
            if (mouseInput != null) //träffade ett target
            {
                //order attack
                OrderAttackUnit(mouseInput, false); //när det är höger klick så är det bara attack på fiender
            }
            else if (mouseHitValid)
            {
                OrderMove(mouseHitPos); //have target enemys aswell in here?
            }
        }
    }

    void GetMoveState()
    {
        if (Input.GetButtonDown("MoveCommand"))
        {
            moveState = MoveState.Move;
            Cursor.SetCursor(moveCursor, hotspot, cursorMode);
        }
        else if (Input.GetButtonDown("AttackMoveCommand"))
        {
            moveState = MoveState.Attack;
            Cursor.SetCursor(attackCursor, hotspot, cursorMode);
        }
        else if (Input.GetButtonDown("Fire1") || (Input.GetButtonDown("Cancel"))) //escape
        {
            moveState = MoveState.Move; //stänger av den igen
            Cursor.SetCursor(moveCursor, hotspot, cursorMode);
        }

    }


    List<Vector3> GetBoxPattern(List<Transform> t, Vector3 posi) //special case för när den bara är 1 eller 2!!!
    {
        //int cols = 5;
        List<Vector3> movePositions = new List<Vector3>();
        int pI = 0;

        int pX = -2;
        int pY = 0;
        for (pI = 0; pI < t.Count;)
        {
            if (pX >= 2)
            {
                pY++;
                pX = -2;
            }
            Vector3 movePosTemp = new Vector3(posi.x + (pX * movePositionsMult), posi.y, posi.z + (pY * movePositionsMult));
            movePositions.Add(movePosTemp);
            pX++;
            pI++;
        }

        return movePositions;
    }

    void PlaceGroundMarker(Vector3 pos)
    {
        for (int i = 0; i < groundMarkerPoolSize; i++)
        {

            if(groundMarkerPool[i].gameObject.activeSelf == false)
            {
                groundMarkerPool[i].position = pos;
                groundMarkerPool[i].GetComponent<FadingLight>().StartFade();
                return;
            }
        }

        groundMarkerPool[roundRobinIndex].position = pos;
        groundMarkerPool[roundRobinIndex].GetComponent<FadingLight>().StartFade();

        roundRobinIndex++; //så den inte ska använda samma gamla hela tiden
        if(roundRobinIndex >= groundMarkerPoolSize)
        {
            roundRobinIndex = 0;
        }
    }
}
