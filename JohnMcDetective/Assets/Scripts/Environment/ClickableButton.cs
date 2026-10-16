using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private LockCode lockCodeReference;
    [SerializeField] private NPCType npcType;
    [SerializeField] private int valueChange = 1;
    
    public void OnPointerClick(PointerEventData eventData)
    {
        lockCodeReference.ChangeCombination(npcType, valueChange);
    }
}
