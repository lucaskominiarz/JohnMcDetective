using UnityEngine;
using UnityEngine.EventSystems;

public class PortalChangeWorld : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Camera.main.GetComponent<ScrollView>().ChangeWorld(); // pas super propre mais ça suffira pour le scope
    }
}
