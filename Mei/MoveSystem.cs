using UnityEngine;

public class MoveSystem : MonoBehaviour
{
    private Vector3 startPosition;
    private bool placedCorrectly = false;
    private bool isDragging = false;

    public Transform[] possibleSlots; // Todos os slots onde pode encaixar
    public Transform correctSlot; // O slot correto
    public float snapDistance = 0.5f;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void OnMouseDown()
    {
        if (!placedCorrectly)
        {
            isDragging = true;
        }
    }

    private void OnMouseDrag()
    {
        if (!placedCorrectly && isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;
            transform.position = mousePosition;
        }
    }

    private void OnMouseUp()
    {
        if (!placedCorrectly)
        {
            Transform closestSlot = null;
            float closestDistance = Mathf.Infinity;

            // Verifica todos os possíveis slots
            foreach (Transform slot in possibleSlots)
            {
                float distance = Vector3.Distance(transform.position, slot.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestSlot = slot;
                }
            }

            if (closestSlot != null && closestDistance < snapDistance)
            {
                transform.position = closestSlot.position;
                placedCorrectly = (closestSlot == correctSlot);

                if (placedCorrectly)
                {
                    Debug.Log($"? Peça {gameObject.name} foi colocada no lugar certo!");
                }
                else
                {
                    Debug.Log($"? Peça {gameObject.name} foi colocada no lugar errado!");
                }
            }
            else
            {
                transform.position = startPosition;
            }

            isDragging = false;
        }
    }

    public bool IsCorrect()
    {
        return placedCorrectly;
    }

    public void ResetPosition()
    {
        transform.position = startPosition;
        placedCorrectly = false;
    }
}
