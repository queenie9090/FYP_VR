using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSFX : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        SoundManager.Instance.PlayHover();
    }

    public void OnPointerClick(PointerEventData eventData)
    { 
        SoundManager.Instance.PlayClick();
    }
}
