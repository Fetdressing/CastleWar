using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Selector))]
public class Builder : MonoBehaviour {
    public GameObject uiBuilderCanvas;

    //different building types
    public GameObject structurePanel;
    public GameObject towerPanel;
    public GameObject specialPanel;

    public Text resourceDisplayer;
    public int startResources = 100;
    private int currResources;
    private Selector selector; //för att hämta input positions osv
    public LayerMask placementLayerMask; //används när man testar att sätta ut byggnader

    public Material invalidPlacementMat;
    public Material validPlacementMat;

    private Building[] currentTypeBuildings; //den som är selectade av de undre
    public Building[] structures;
    public Building[] towers;
    public Building[] specials;

    private int currBuildingIndex = 0;
    private Building currBuildingSel;

	void Start () {
        Init();
	}

    public void Init()
    {
        currResources = startResources;
        AddResources(0); //updaterar

        selector = this.transform.GetComponent<Selector>();


        InstantiatePlacementShowObjects();
        ChangeBuildingIndex(100000);
        GenerateBuildingUI();
        ChangeBuildingPanel(1); //set default panelen igång
    }

    // Update is called once per frame
    void Update ()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            ChangeBuildingIndex(10000);
        }


        if(structurePanel.activeSelf == true) //hitta den typen som är vald just nu
        {
            currentTypeBuildings = structures;
        }
        else if(towerPanel.activeSelf == true)
        {
            currentTypeBuildings = towers;
        }
        else if(specialPanel.activeSelf == true)
        {
            currentTypeBuildings = specials;
        }

        if (currBuildingIndex < currentTypeBuildings.Length)
        {
            currBuildingSel = currentTypeBuildings[currBuildingIndex];

            if (currBuildingSel.placementShowObj.activeSelf == false)
            {
                currBuildingSel.placementShowObj.SetActive(true);
            }

            currBuildingSel.placementShowObj.transform.position = selector.mouseHitPos;


            if (IsViablePlacement(currBuildingSel, selector.mouseHitPos))
            {
                currBuildingSel.placementShowObj.GetComponent<Renderer>().material = validPlacementMat;

                if (!EventSystem.current.IsPointerOverGameObject()) //hovrar jag över ui?
                { // UI elements getting the hit/hover
                    if (Input.GetMouseButtonDown(0))
                    {
                        PlaceBuilding(currBuildingSel, selector.mouseHitPos);
                    }
                }
            }
            else
            {
                currBuildingSel.placementShowObj.GetComponent<Renderer>().material = invalidPlacementMat;
            }
        }
        else
        {
            if (currBuildingSel.placementShowObj != null && currBuildingSel.placementShowObj.activeSelf == true)
            {
                currBuildingSel.placementShowObj.SetActive(false);
            }
        }
    }

    bool IsViablePlacement(Building b, Vector3 middlePoint)
    {
        RaycastHit hit;
        foreach (Transform child in b.placementShowObj.transform)
        {
            if(Physics.Raycast(child.position, Vector3.down, out hit, 3, placementLayerMask))
            {
                //Debug.Log(hit.collider.gameObject.name);
                if(hit.collider.gameObject.layer != LayerMask.NameToLayer("Terrain"))
                {
                    return false;
                }
                if (hit.collider.gameObject.tag != "Ground")
                {
                    return false;
                }
            }
        }
        Bounds boundBox = b.placementObject.GetComponent<Renderer>().bounds; //gör denna checken oxå somehow!
        //float largestExtent = Mathf.Max(boundBox.extents.y, Mathf.Max(boundBox.extents.x, boundBox.extents.z));

        Collider[] hitColliders = Physics.OverlapBox(middlePoint, boundBox.extents, Quaternion.identity, placementLayerMask);
        for(int i = 0; i < hitColliders.Length; i++)
        {
            if (hitColliders[i].gameObject.layer != LayerMask.NameToLayer("Terrain"))
            {
                return false;
            }
            if (hitColliders[i].gameObject.tag != "Ground")
            {
                return false;
            }
        }

        return true;
    }

    void PlaceBuilding(Building b, Vector3 pos)
    {
        if (AddResources(-b.cost))
        {
            GameObject tempB = Instantiate(b.spawnObject);
            tempB.layer = selector.playerLayer;
            tempB.transform.position = pos;
            tempB.GetComponent<AIBase>().Init();
        }
    }

    bool AddResources(int r) //returnerar ifall den kunde spendera summan
    {
        if ((currResources + r) >= 0)
        {
            currResources += r;
            resourceDisplayer.text = currResources.ToString();
            return true;
        }

        resourceDisplayer.text = currResources.ToString();
        return false;
    }

    public void ChangeBuildingIndex(int i)
    {
        if (i < 0)
        {
            i = 0;
        }
        for(int it = 0; it < structures.Length; it++)
        {
            structures[it].placementShowObj.SetActive(false);
        }
        for (int it = 0; it < towers.Length; it++)
        {
            towers[it].placementShowObj.SetActive(false);
        }
        for (int it = 0; it < specials.Length; it++)
        {
            specials[it].placementShowObj.SetActive(false);
        }
        currBuildingIndex = i;
        //Debug.Log(i.ToString());
    }

    [System.Serializable]
    public struct Building
    {
        [HideInInspector]
        public int index; //så man kan matcha torn med bild

        public GameObject spawnObject;
        public GameObject placementObject;
        [HideInInspector]
        public GameObject placementShowObj;
        public int cost;

        public Sprite uiImage;
        public GameObject buildingBottom;
        //public float buildingPlacementSize;
    }


    void GenerateBuildingUI()
    {
        for(int i = 0; i < structures.Length; i++)
        {
            GameObject tempB = Instantiate(structures[i].buildingBottom.gameObject) as GameObject;
            int indexB = i;
            tempB.GetComponent<Button>().onClick.AddListener(() => { ChangeBuildingIndex(indexB); });

            if (tempB.GetComponent<Image>().sprite != null)
            {
                tempB.GetComponent<Image>().sprite = structures[i].uiImage;
            }

            tempB.transform.SetParent(structurePanel.transform, false); //positionera dem på nått nice sett!
        }

        for (int i = 0; i < towers.Length; i++)
        {
            GameObject tempB = Instantiate(towers[i].buildingBottom.gameObject) as GameObject;
            int indexB = i;
            tempB.GetComponent<Button>().onClick.AddListener(() => { ChangeBuildingIndex(indexB); });

            if (tempB.GetComponent<Image>().sprite != null)
            {
                tempB.GetComponent<Image>().sprite = towers[i].uiImage;
            }

            tempB.transform.SetParent(towerPanel.transform, false); //positionera dem på nått nice sett!
        }

        for (int i = 0; i < specials.Length; i++)
        {
            GameObject tempB = Instantiate(specials[i].buildingBottom.gameObject) as GameObject;
            int indexB = i;
            tempB.GetComponent<Button>().onClick.AddListener(() => { ChangeBuildingIndex(indexB); });

            if (tempB.GetComponent<Image>().sprite != null)
            {
                tempB.GetComponent<Image>().sprite = specials[i].uiImage;
            }

            tempB.transform.SetParent(specialPanel.transform, false); //positionera dem på nått nice sett!
        }
    }

    void InstantiatePlacementShowObjects()
    {
        for (int i = 0; i < structures.Length; i++)
        {
            structures[i].placementShowObj = Instantiate(structures[i].placementObject);
            structures[i].index = i; //ge dem sina indexes så att rätt torn väljs till rätt bild
        }

        for (int i = 0; i < towers.Length; i++)
        {
            towers[i].placementShowObj = Instantiate(towers[i].placementObject);
            towers[i].index = i; //ge dem sina indexes så att rätt torn väljs till rätt bild
        }

        for (int i = 0; i < specials.Length; i++)
        {
            specials[i].placementShowObj = Instantiate(specials[i].placementObject);
            specials[i].index = i; //ge dem sina indexes så att rätt torn väljs till rätt bild
        }
    }

    public void ChangeBuildingPanel(int i)
    {
        ChangeBuildingIndex(10000); //så den av-aktiverar den gamla panelens towerplacementobject
        switch (i)
        {
            case 1:
                structurePanel.SetActive(true);
                towerPanel.SetActive(false);
                specialPanel.SetActive(false);
                break;
            case 2:
                structurePanel.SetActive(false);
                towerPanel.SetActive(true);
                specialPanel.SetActive(false);
                break;
            case 3:
                structurePanel.SetActive(false);
                towerPanel.SetActive(false);
                specialPanel.SetActive(true);
                break;
            default:
                structurePanel.SetActive(true);
                towerPanel.SetActive(false);
                specialPanel.SetActive(false);
                break;
        }
    }

    public void ReportDeadUnit(Transform deadUnit, int resourceWorth)
    {
        if(selector.playerTeam != LayerMask.LayerToName(deadUnit.gameObject.layer))
        {
            AddResources(resourceWorth);
        }
    }
}
