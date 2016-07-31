using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Selector selector;

    public string tooltip = "Information missing!!!";
    [HideInInspector]
    public int index;

    void Start()
    {
        selector = GameObject.FindGameObjectWithTag("PlayerHandler").GetComponent<Selector>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltip = selector.GetSpellToolTip(index);
        selector.ToggleToolTipSpell(tooltip);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        selector.ToggleToolTipSpell("");
    }
}
