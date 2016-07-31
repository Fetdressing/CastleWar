using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Selector : MonoBehaviour {
    private Transform thisTransform;
    private TeamHandler teamHandler;
    [HideInInspector]
    public string playerTeam;
    [HideInInspector]
    public LayerMask playerLayer;

    [HideInInspector]
    public RaycastHit mouseHit;
    public LayerMask selectLayerMask = -1; //vad som ska träffas som target
    public LayerMask mouseHitLayerMask; //vad som ska träffas som mark
    [HideInInspector]
    public Vector3 mouseHitPos = Vector3.zero;
    //private int selMask;

    private List<Transform> targets = new List<Transform>();
    private List<Health> targetHealths = new List<Health>(); //för att kunna hämta unitsizes
    private List<AIBase> targetAIBases = new List<AIBase>();

    private List<List<TargetInGroup>> targetGroups = new List<List<TargetInGroup>>(); //sorterade i grupper efter unit type
    private int currTargetGroupIndex = 0; //indexet till den targetGroupen som ska användas

    private List<Transform> currTargetGroup = new List<Transform>();
    private int targetGroupUnitSpellcasterIndex = 0; //det unitet i targetGroupen som ska kasta spell härnäst
    //selectionBox
    //public GameObject selectionBoxCanvas;
    public RectTransform selectionBox;
    private Vector2 initialSelectionBoxAnchor = Vector2.zero;
    //selectionBox

    //UI
    public GameObject unitInfoCanvas;
    public Text currHealthText;
    public Text maxHealthText;
    public Text armorText;
    public Text hpRegText;
    public Text minDamageText;
    public Text maxDamageText;
    public Image unitPortrait;

    public Button[] spellButtons = new Button[4];
    public GameObject spellTooltipTextObject;
    private int selectedSpellIndex = -1000;
    public Transform spellCastMarkObject; //följer musen när man har spell redo
    public Sprite spellMissingSprite; //ifall det inte finns någon spell så används denna spriten istället

    enum MoveState { Move, Attack, Patrol };
    private MoveState moveState = MoveState.Move;

    private float movePositionsMult = 3; // hur långt ifrån de ska ställa sig från varandra

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

    public Material attackSelMat; //materialet när man orderar attack på unit
    // Use this for initialization
	
	// Update is called once per frame
	void Update () {
        SelectorUpdate();
    }

    public virtual void Init()
    {
        thisTransform = this.transform;
        teamHandler = GameObject.FindGameObjectWithTag("TeamHandler").GetComponent<TeamHandler>();
        playerTeam = teamHandler.playerTeam;
        playerLayer = LayerMask.NameToLayer(playerTeam);

        Cursor.SetCursor(moveCursor, hotspot, cursorMode);

        for (int i = 0; i < groundMarkerPoolSize; i++)
        {
            GameObject temp = GameObject.Instantiate(groundMarkerObject.gameObject);
            temp.SetActive(false);
            groundMarkerPool.Add(temp.transform);
        }
        currTargetGroupIndex = 0;
        InitSpellUI();
        //selMask = selectLayerMask.value;
    }

    public void SelectorUpdate()
    {
        SelectionBox();
        CheckTargetsAlive();

        GetMouseInput();


        if (targets.Count != 0 && currTargetGroup.Count != 0)
        {
            unitInfoCanvas.SetActive(true);
        }
        else
        {
            unitInfoCanvas.SetActive(false);
        }
        GetMoveState(); //behöver vara sist
        UpdateUnitInfo();
        UpdateCurrTargetGroup();
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

        if (Physics.Raycast(ray, out mouseHit, Mathf.Infinity, mouseHitLayerMask))
        {
            pos = mouseHit.point;
            return true;
        }
            return false;
    }

    void CheckTargetsAlive()
    {
        for(int i = 0; i < targets.Count; i++) //tömmer sig listan själv?
        {
            if(targets[i] == null || targets[i].gameObject.activeSelf == false || !targetHealths[i].IsAlive())
            {
                RemoveTarget(i);
            }
        }
    }

    void AddTarget(Transform newTarget)
    {
        if (newTarget != null)
        {
            bool exists = false; //kolla så att den inte redan är med i listan
            for (int i = 0; i < targets.Count; i++)
            {
                //Debug.Log(targets[i].ToString());
                if (newTarget == targets[i])
                {
                    exists = true;
                }
            }

            if (exists == false)
            {
                Health tempHealth = newTarget.GetComponent<Health>();
                tempHealth.ToggleSelMarker(true);

                AIBase tempAIBase = null;
                if(newTarget.GetComponent<AIBase>() != null)
                {
                    tempAIBase = newTarget.GetComponent<AIBase>();
                }
                targets.Add(newTarget);
                targetHealths.Add(tempHealth);
                targetAIBases.Add(tempAIBase); //måste addas även om den är null
                int targetListID = targets.Count - 1; //kan vara större eller mindre
                //lägg till det i en grupp, men kolla först om gruppen finns
                bool groupExists = false;
                int existingID = 0;
                TargetInGroup tempTiG = new TargetInGroup();
                tempTiG.target = newTarget;
                tempTiG.targetListIndex = targetListID; //behövs för att hitta den i den grupperade listan sedan
                tempTiG.health = tempHealth; //health innehåller unitID
                for (int i = 0; i < targetGroups.Count; i++)
                {
                    if (tempHealth.unitID == targetGroups[i][0].health.unitID) //finns redan
                    {
                        groupExists = true;
                        existingID = i;
                        break;
                    }
                }
                if (groupExists)
                {
                    targetGroups[existingID].Add(tempTiG); //lägg till den i korrekt grupp
                }
                else
                {
                    List<TargetInGroup> tempList = new List<TargetInGroup>(); //skapa en ny för det fanns inga av liknande units
                    tempList.Add(tempTiG);
                    targetGroups.Add(tempList);
                }
            }
        }
    }

    void RemoveTarget(int i) //removes target by index
    {
        if (targets.Count <= i) return;

        if (targets[i] != null)
        {
            targets[i].GetComponent<Health>().ToggleSelMarker(false);
        }
        int unitID = targetHealths[i].unitID;
        targets.RemoveAt(i);
        targetHealths.RemoveAt(i);
        targetAIBases.RemoveAt(i);

        for(int y = 0; y < targetGroups.Count; y++) //felet probably!!!
        {
            if(targetGroups[y][0].health.unitID == unitID) //hitta listan som innehåller samma sorts units som den jag vill ta bort
            {
                for(int k = 0; k < targetGroups[y].Count; k++)
                {
                    if(targetGroups[y][k].targetListIndex == i) //sen kan jag bara leta efter det nedsparade indexet (y)
                    {
                        targetGroups[y].RemoveAt(k);
                        break;
                    }
                }
                break;
            }
        }
    }

    void ClearTargets()
    {
        for(int i = 0; i < targets.Count; i++)
        {
            targets[i].GetComponent<Health>().ToggleSelMarker(false);            
        }
        targets.Clear();
        targetHealths.Clear();
        targetAIBases.Clear();
        
        for(int i = 0; i < targetGroups.Count; i++)
        {
            targetGroups[i].Clear();
        }
        targetGroups.Clear();
        currTargetGroup.Clear();
        UpdateCurrTargetGroup();
        GetNextTargetGroupIndex(); //för indexet blir wack efter man clearat
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
        Vector3 originPos = pos;
        PlaceGroundMarker(pos + new Vector3(0, 0.2f, 0));
        if (moveState == MoveState.Move) //annars ska den bara avmarkera den andra statet, som typ attack
        {
            List<Vector3> movePositions = new List<Vector3>();

            if (targets.Count > 2) //använd bara box pattern när det är fler än 2
            {
                movePositions = GetBoxPattern(targets, targetHealths, pos);
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
                        if (targets[i].GetComponent<SpawnBuilding>() != null)
                        {
                            targets[i].GetComponent<AIBase>().AddCommandToList(originPos, AIBase.UnitState.Moving, targets[i].transform, false, selectedSpellIndex);
                        }
                        else
                        {
                            targets[i].GetComponent<AIBase>().AddCommandToList(movePositions[i], AIBase.UnitState.Moving, targets[i].transform, false, selectedSpellIndex);
                        }
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

                        if (targets[i].GetComponent<SpawnBuilding>() != null)
                        {
                            targets[i].GetComponent<AIBase>().Move(originPos); //kör på origin för building rallypoint
                        }
                        else
                        {
                            targets[i].GetComponent<AIBase>().Move(movePositions[i]);
                        }
                    }
                }
            }
        }
        moveState = MoveState.Move;
        Cursor.SetCursor(moveCursor, hotspot, cursorMode);
    }

    void OrderAttackMove(Vector3 pos)
    {
        Vector3 originPos = pos;
        PlaceGroundMarker(pos + new Vector3(0, 0.2f, 0));

        List<Vector3> movePositions = new List<Vector3>();
        if (targets.Count > 2) //använd bara box pattern när det är fler än 2
        {
            movePositions = GetBoxPattern(targets, targetHealths, pos);
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
                    if (targets[i].GetComponent<SpawnBuilding>() != null)
                    {
                        targets[i].GetComponent<AIBase>().AddCommandToList(originPos, AIBase.UnitState.AttackMoving, targets[i].transform, false, selectedSpellIndex);
                    }
                    else
                    {
                        targets[i].GetComponent<AIBase>().AddCommandToList(movePositions[i], AIBase.UnitState.AttackMoving, targets[i].transform, false, selectedSpellIndex); //skicka sig själv som target så inget fuckar
                    }
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

                    if (targets[i].GetComponent<SpawnBuilding>() != null) //kör på origin för building rallypoint
                    {
                        targets[i].GetComponent<AIBase>().AttackMove(originPos);
                    }
                    else
                    {
                        targets[i].GetComponent<AIBase>().AttackMove(movePositions[i]);
                    }
                }
            }
        }

        moveState = MoveState.Move;
        Cursor.SetCursor(moveCursor, hotspot, cursorMode);
    }

    void OrderAttackUnit(Transform t, bool friendfire)
    {
        PlaceGroundMarker(t.position + new Vector3(0, 0.2f, 0));
        t.GetComponent<Health>().ApplyMaterial(attackSelMat, 0.5f);
        if (Input.GetButton("Add"))
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i].GetComponent<AIBase>() != null)
                {
                    targets[i].GetComponent<AIBase>().AddCommandToList(t.position, AIBase.UnitState.AttackingUnit, t, friendfire, selectedSpellIndex);
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

    void OrderCastSpell(Vector3 pos) //behöver ej sätta ett moveState då det endast är en snubbe som kastar spell
    {
        if (targetGroupUnitSpellcasterIndex >= currTargetGroup.Count)
        {
            targetGroupUnitSpellcasterIndex = 0;
        }

        if (currTargetGroup.Count == 0) return;
        if (currTargetGroup[targetGroupUnitSpellcasterIndex].GetComponent<UnitSpellHandler>() == null) return; //räcker jag kollar en i gruppen då alla är likadana

        if (currTargetGroup[targetGroupUnitSpellcasterIndex].GetComponent<UnitSpellHandler>().SpellIndexExists(selectedSpellIndex) == false) return; //finns spellen på detta unit?

        int nrTries = 0;
        int maxTries = currTargetGroup.Count;
        while(currTargetGroup[targetGroupUnitSpellcasterIndex].GetComponent<UnitSpellHandler>().IsSpellReady(selectedSpellIndex, 10000) == false && nrTries < maxTries) //hitta en caster i gruppen som är redo med spellen
        {
            GetNextSpellCasterIndex();
            nrTries++;
        }

        //Debug.Log(currTargetGroup[targetGroupUnitSpellcasterIndex].name + " " + targetGroupUnitSpellcasterIndex.ToString());

        if (Input.GetButton("Add")) //om add så lägg till kommandot i listan
        {
            if (currTargetGroup[targetGroupUnitSpellcasterIndex].GetComponent<AIBase>() != null)
            {
                currTargetGroup[targetGroupUnitSpellcasterIndex].GetComponent<AIBase>().AddCommandToList(pos, AIBase.UnitState.CastingSpell, currTargetGroup[targetGroupUnitSpellcasterIndex], false, selectedSpellIndex);
            }            
        }
        else
        {
            if (currTargetGroup[targetGroupUnitSpellcasterIndex].GetComponent<AIBase>() != null)
            {
                currTargetGroup[targetGroupUnitSpellcasterIndex].GetComponent<AIBase>().ClearCommands();
                currTargetGroup[targetGroupUnitSpellcasterIndex].GetComponent<AIBase>().PerformSpell(pos, selectedSpellIndex);
            }
        }
        moveState = MoveState.Move;
        targetGroupUnitSpellcasterIndex++;
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
        //mouseHitPos = Vector3.zero;
        Transform mouseInput = GetMouseTarget();
        bool mouseHitValid = GetMousePosition(ref mouseHitPos);
        UpdateSpellMarker(mouseHitValid, mouseHitPos);

        if (Input.GetButtonDown("Fire1")) //attack unit om man har MoveState.Attack och ifall mouseInput != null!!!!!!!!!!!!!!!!!!!!!!!
        {
            if(selectedSpellIndex >= 0 && mouseHitValid) //spell redo
            {
                OrderCastSpell(mouseHitPos);
                selectedSpellIndex = -10000;
            }
            else if (moveState == MoveState.Attack && mouseInput != null)
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
            selectedSpellIndex = -1000; //stäng av spell kasten
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

        if(Input.GetButtonDown("Cancel"))
        {
            ClearTargets();
            selectedSpellIndex = -1000;
        }

        if(Input.GetButtonDown("Spell1")) //Q
        {
            selectedSpellIndex = 0;
        }
        else if(Input.GetButtonDown("Spell2")) //W
        {
            selectedSpellIndex = 1;
        }
        else if (Input.GetButtonDown("Spell3")) //E
        {
            selectedSpellIndex = 2;
        }
        else if (Input.GetButtonDown("Spell4")) //R
        {
            selectedSpellIndex = 3;
        }

        if(Input.GetButtonDown("Next")) //tab
        {
            selectedSpellIndex = -1000;
            GetNextTargetGroupIndex();
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


    List<Vector3> GetBoxPattern(List<Transform> t, List<Health> ht, Vector3 posi) //special case för när den bara är 1 eller 2!!!
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
            Vector3 movePosTemp = new Vector3(posi.x + (pX * ht[pI].unitSize * movePositionsMult), posi.y, posi.z + (pY * ht[pI].unitSize * movePositionsMult)); //*movePositionsMult så den ska vara i lämpligt mått
            movePositions.Add(movePosTemp);

            if (t[pI].GetComponent<BuildingBase>() == null) //byggnader ska inte ta plats i formationen
            {
                pX++;
            }

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
    void UpdateSpellMarker(bool validPos, Vector3 mousePos)
    {
        if (validPos && selectedSpellIndex >= 0 && currTargetGroup.Count != 0)
        {
            if (currTargetGroup[0].GetComponent<UnitSpellHandler>() != null && currTargetGroup[0].GetComponent<UnitSpellHandler>().SpellIndexExists(selectedSpellIndex))
            {
                spellCastMarkObject.gameObject.SetActive(true);
            }
            else
            {
                spellCastMarkObject.gameObject.SetActive(false);
            }
        }
        else
        {
            spellCastMarkObject.gameObject.SetActive(false);
        }
        spellCastMarkObject.position = mousePos + new Vector3(0, 0.5f, 0);
    }

    void UpdateUnitInfo()
    {
        //if (currTargetGroup.Count == 0) return;
        if (targets.Count != 0 && currTargetGroup.Count != 0)
        {
            Health selHealth = currTargetGroup[0].GetComponent<Health>(); //!!!!!kanske använda det indexet som spellselected har, dvs visa health för den som kastar spellen!!!!!
            if(selHealth.unitSprite != null)
            {
                unitPortrait.sprite = selHealth.unitSprite;
            }
            else
            {
                unitPortrait.sprite = null;
            }

            currHealthText.text = selHealth.GetCurrHealth().ToString();
            maxHealthText.text = "/" + selHealth.maxHealth.ToString();

            armorText.text = selHealth.armor.ToString();
            hpRegText.text = selHealth.healthRegAmount.ToString();

            if(currTargetGroup[0].GetComponent<AIBase>() != null)
            {
                AIBase selAIBase = currTargetGroup[0].GetComponent<AIBase>();
                minDamageText.text = selAIBase.GetMinDamage().ToString();
                maxDamageText.text = selAIBase.GetMaxDamage().ToString();
            }
            else
            {
                minDamageText.text = "N/A";
                maxDamageText.text = "/" + "N/A";
            }

            UpdateSpellInfo();
        }
        else
        {
            currHealthText.text = "N/A";
            maxHealthText.text = " N/A";

            armorText.text = " N/A";
            hpRegText.text = " N/A";

            minDamageText.text = "N/A";
            maxDamageText.text = "/" + "N/A";
        }
    }
    void UpdateSpellInfo()
    {
        if (currTargetGroup[0].GetComponent<UnitSpellHandler>() != null)
        {
            UnitSpellHandler tempUSH = currTargetGroup[0].GetComponent<UnitSpellHandler>();
            for (int i = 0; i < spellButtons.Length; i++)
            {
                if (currTargetGroup[0].GetComponent<UnitSpellHandler>().SpellIndexExists(i))
                {
                    spellButtons[i].GetComponent<Tooltip>().tooltip = tempUSH.allAbilities[i].name + "\n" + tempUSH.allAbilities[i].tooltip;
                    spellButtons[i].GetComponent<Image>().sprite = tempUSH.allAbilities[i].abilitySprite;
                }
                else
                {
                    spellButtons[i].GetComponent<Tooltip>().tooltip = "";
                    spellButtons[i].GetComponent<Image>().sprite = spellMissingSprite;
                }
            }
        }
        else
        {
            for (int i = 0; i < spellButtons.Length; i++)
            {
                spellButtons[i].GetComponent<Tooltip>().tooltip = "";
                spellButtons[i].GetComponent<Image>().sprite = spellMissingSprite;
            }
        }
    }

    void GetNextTargetGroupIndex()
    {
        currTargetGroupIndex++;
        if(currTargetGroupIndex >= targetGroups.Count)
        {
            currTargetGroupIndex = 0;
        }
        UpdateCurrTargetGroup();
        //Debug.Log(currTargetGroupIndex.ToString());
    }
    void UpdateCurrTargetGroup()
    {
        currTargetGroup.Clear();
        if (targetGroups.Count > currTargetGroupIndex) //kolla så indexet finns
        {
            for (int i = 0; i < targetGroups[currTargetGroupIndex].Count; i++)
            {
                currTargetGroup.Add(targetGroups[currTargetGroupIndex][i].target);
            }
        }
    }

    void GetNextSpellCasterIndex()
    {
        targetGroupUnitSpellcasterIndex++;
        if(targetGroupUnitSpellcasterIndex >= currTargetGroup.Count)
        {
            targetGroupUnitSpellcasterIndex = 0;
        }
    }

    class TargetInGroup //används för att hålla koll på grupperingslistan bara
    {
        public Transform target;
        public int targetListIndex;
        public Health health;
    }

    public void InitSpellUI()
    {
        //unitInfoCanvas.SetActive(true);
        //for (int i = 0; i < spellButtons.Length; i++)
        //{
        //    EventTrigger eT = spellButtons[i].gameObject.GetComponent<EventTrigger>() as EventTrigger;

        //    EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        //    entryEnter.eventID = EventTriggerType.PointerEnter;
        //    entryEnter.callback.AddListener((eventData) => { this.ToggleToolTipSpell( spellButtons[i].gameObject.GetComponent<Tooltip>().tooltip); });

        //    EventTrigger.Entry entryLeave = new EventTrigger.Entry();
        //    entryLeave.eventID = EventTriggerType.PointerExit;
        //    entryLeave.callback.AddListener((eventData) => { this.ToggleToolTipSpell( spellButtons[i].gameObject.GetComponent<Tooltip>().tooltip); });

        //    eT.triggers[0] = (entryEnter);
        //    eT.triggers[1] = (entryLeave);
        //}
        //unitInfoCanvas.SetActive(false);

        for (int i = 0; i < spellButtons.Length; i++)
        {
            spellButtons[i].GetComponent<Tooltip>().index = i;
        }
    }

    public string GetSpellToolTip(int index) //kallas från tooltip scriptet
    {
        return(spellButtons[index].GetComponent<Tooltip>().tooltip);
    }

    public void ToggleToolTipSpell(string tooltip) //kallas från tooltip scriptet
    {
        if(tooltip == "")
        {
            spellTooltipTextObject.SetActive(false);
            return;
        }
        spellTooltipTextObject.SetActive(true);
        spellTooltipTextObject.GetComponentsInChildren<Text>()[0].text = tooltip;
    }
}
