using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class ExtensionPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject extension;

    public void Start()
    {
        extension.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        extension.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        extension.SetActive(false);
    }
}