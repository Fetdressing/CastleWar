using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Selector))]
public class Builder : MonoBehaviour {
    int startResources = 100;
    int currResources;
    private Selector selector; //för att hämta input positions osv
    public LayerMask placementLayerMask; //används när man testar att sätta ut byggnader

    public Material invalidPlacementMat;
    public Material validPlacementMat;

    public Building[] buildings;
    
	void Start () {
        Init();
	}

    public void Init()
    {
        selector = this.transform.GetComponent<Selector>();

        for(int i = 0; i < buildings.Length; i++)
        {
            buildings[i].placementShowObj = Instantiate(buildings[i].placementObject);
        }
    }

    // Update is called once per frame
    void Update ()
    {
        buildings[0].placementShowObj.transform.position = selector.mouseHitPos;

        if(IsViablePlacement(buildings[0], selector.mouseHitPos))
        {
            buildings[0].placementShowObj.GetComponent<Renderer>().material = validPlacementMat;
        }
        else
        {
            buildings[0].placementShowObj.GetComponent<Renderer>().material = invalidPlacementMat;
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


        return true;
    }

    void PlaceBuilding(Building b, Vector3 pos)
    {
        GameObject tempB = Instantiate(b.spawnObject);
        tempB.layer = selector.playerLayer;
    }

    [System.Serializable]
    public struct Building
    {
        public GameObject spawnObject;
        public GameObject placementObject;
        [HideInInspector]
        public GameObject placementShowObj;
        public Sprite uiImage;
        public int cost;
        //public float buildingPlacementSize;
    }
}
