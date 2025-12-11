using UnityEngine;
using UnityEngine.Rendering;

public class DesativarColliderItem : MonoBehaviour
{
    [Header("Collider do Item")]
    public Collider2D colliderItem; // arraste o collider do item aqui

    // Chame esta função quando quiser desativar o collider

    private void Start()
    {
        DesativarCollider();
    }
    public void DesativarCollider()
    {
        if (colliderItem != null)
        {
            colliderItem.enabled = false;
        }
    }
}
