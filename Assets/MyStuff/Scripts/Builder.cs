using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Selector))]
public class Builder : MonoBehaviour {
    int startResources = 100;
    int currResources;
    private Selector selector; //för att hämta input positions osv

    public Building[] buildings;
    
	void Start () {
        Init();
	}

    public void Init()
    {
        selector = this.transform.GetComponent<Selector>();
    }

    // Update is called once per frame
    void Update ()
    {

	}

    bool IsViablePlacement(Building b, Vector3 middlePoint)
    {
        RaycastHit hit;
        foreach (Transform child in b.placementObject.transform)
        {
            if(Physics.Raycast(child.position, Vector3.down, out hit, 3))
            {
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
        Bounds boundBox = b.placementObject.GetComponent<Renderer>().bounds;


        return true;
    }

    [System.Serializable]
    public struct Building
    {
        public GameObject spawnObject;
        public GameObject placementObject;
        public Sprite uiImage;
        public int cost;
        public float buildingPlacementSize;
    }
}
