using System.Collections;
using UnityEngine;

public class WaypointItemActivator : MonoBehaviour
{
    [System.Serializable]
    public class PlayerWaypoint
    {
        public Transform player;           // O player
        public Transform waypoint;         // O waypoint do player
        public GameObject itemToActivate;  // O item que aparecerá
        public float activationDistance = 1f; // Distância mínima para ativar
        [HideInInspector] public bool activated = false;
    }

    public PlayerWaypoint playerWaypoint;

    [Header("Configuração do Fade")]
    public float fadeDuration = 1f; // Tempo para o fade in

    void Start()
    {
        // Inicialmente deixa o item invisível
        if (playerWaypoint.itemToActivate != null)
        {
            SpriteRenderer sr = playerWaypoint.itemToActivate.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 0f;
                sr.color = c;
            }
            playerWaypoint.itemToActivate.SetActive(true); // precisa estar ativo para o fade
        }
    }

    void Update()
    {
        if (playerWaypoint.activated) return; // se já ativou, não faz nada

        // Verifica a distância entre player e waypoint
        float distance = Vector3.Distance(playerWaypoint.player.position, playerWaypoint.waypoint.position);
        if (distance <= playerWaypoint.activationDistance)
        {
            playerWaypoint.activated = true;
            StartCoroutine(FadeInItem(playerWaypoint.itemToActivate));
        }
    }

    IEnumerator FadeInItem(GameObject item)
    {
        if (item == null) yield break;

        SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        float elapsed = 0f;
        Color c = sr.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / fadeDuration);
            sr.color = c;
            yield return null;
        }

        c.a = 1f;
        sr.color = c; // garante que termine totalmente visível
    }
}
