using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Selector))]
public class Builder : MonoBehaviour {
    public GameObject uiBuilderCanvas;
    public GameObject buildingPanel;

    public Text resourceDisplayer;
    public int startResources = 100;
    private int currResources;
    private Selector selector; //för att hämta input positions osv
    public LayerMask placementLayerMask; //används när man testar att sätta ut byggnader

    public Material invalidPlacementMat;
    public Material validPlacementMat;

    public Building[] buildings;
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

        for(int i = 0; i < buildings.Length; i++)
        {
            buildings[i].placementShowObj = Instantiate(buildings[i].placementObject);
            buildings[i].index = i; //ge dem sina indexes så att rätt torn väljs till rätt bild
        }
        ChangeBuildingIndex(1);
        GenerateBuildingUI();
    }

    // Update is called once per frame
    void Update ()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            ChangeBuildingIndex(10000);
        }

        if (currBuildingIndex < buildings.Length)
        {
            currBuildingSel = buildings[currBuildingIndex];

            if (currBuildingSel.placementShowObj.activeSelf == false)
            {
                currBuildingSel.placementShowObj.SetActive(true);
            }

            currBuildingSel.placementShowObj.transform.position = selector.mouseHitPos;


            if (IsViablePlacement(currBuildingSel, selector.mouseHitPos))
            {
                currBuildingSel.placementShowObj.GetComponent<Renderer>().material = validPlacementMat;

                if(Input.GetMouseButtonDown(0))
                {
                    PlaceBuilding(currBuildingSel, selector.mouseHitPos);
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
        for(int i = 0; i < buildings.Length; i++)
        {
            GameObject tempB = Instantiate(buildings[i].buildingBottom.gameObject);
            tempB.GetComponent<Button>().onClick.AddListener(() => { ChangeBuildingIndex(0); });
            tempB.transform.SetParent(buildingPanel.transform, false); //positionera dem på nått nice sett!
        }
    }
}
