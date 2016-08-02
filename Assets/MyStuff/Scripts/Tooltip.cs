using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Selector selector;

    [HideInInspector]
    public string tooltip = "Information missing!!!";
    [HideInInspector]
    public int index;

    void Start()
    {
        selector = GameObject.FindGameObjectWithTag("PlayerHandler").GetComponent<Selector>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //tooltip = selector.GetSpellToolTip(index);
        selector.ToggleToolTip(tooltip);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        selector.ToggleToolTip("");
    }

    public void ChangeToolTip(string s) //så får andra skript kalla på denna istället när värdena ändras
    {
        tooltip = s;
    }
}
