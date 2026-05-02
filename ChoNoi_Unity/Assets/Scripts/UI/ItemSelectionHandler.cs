using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSelectionHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private float verticalMoveAmount = 30f;
    [SerializeField] private float moveTime = 0.1f;
    [Range(0f, 2f), SerializeField] private float scaleAmount = 1.1f;

    private Vector3 startPos;
    private Vector3 startScale;

    private void Start()
    {
        startPos = transform.position;
        startScale = transform.localScale;
    }

    private IEnumerator MoveItem(bool startingAnimation)
    {
        Vector3 endPosition = startPos;
        Vector3 endScale = startScale;

        float elapsedTime = 0f;
        while (elapsedTime < moveTime)
        {
            elapsedTime += Time.deltaTime;

            if (startingAnimation)
            {
                endPosition = startPos + new Vector3(0f, verticalMoveAmount, 0f);
                endScale = startScale * scaleAmount;
            }
            else
            {
                endPosition = startPos;
                endScale = startScale;
            }
        }

        // Calc lerp amounts
        Vector3 lerpedPos = Vector3.Lerp(transform.position, endPosition, (elapsedTime / moveTime));
        Vector3 lerpedScale = Vector3.Lerp(transform.position, endScale, (elapsedTime / moveTime));

        // Apply changes to the position and scale
        transform.position = lerpedPos;
        transform.localScale = lerpedScale;

        yield return null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Select item
        eventData.selectedObject = gameObject;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        eventData.selectedObject = null;
    }

    public void OnSelect(BaseEventData eventData)
    {
        StartCoroutine(MoveItem(true));
    }

    public void OnDeselect(BaseEventData eventData)
    {
        StartCoroutine(MoveItem(false));
    }
}
