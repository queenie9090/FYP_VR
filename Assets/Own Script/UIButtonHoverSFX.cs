using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonHoverSFX : MonoBehaviour, IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        SoundManager.Instance.PlayHover();
    }
}
