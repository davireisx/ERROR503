using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VerificacaoSimples : MonoBehaviour
{
    [System.Serializable]
    public class CharacterWaypoint
    {
        public Transform character;
        public Transform waypoint;
        public float activationDistance = 1f;
        [HideInInspector] public bool isAtWaypoint = false;
    }

    [Header("Personagens e Waypoints")]
    public List<CharacterWaypoint> characterWaypoints = new List<CharacterWaypoint>();

    [Header("Item para Fade In")]
    public SpriteRenderer itemSprite;
    public float fadeSpeed = 2f;

    [Header("Diálogo")]
    public GameObject dialogueObject;

    private bool itemActivated = false;
    private bool dialogueActivated = false;

    void Update()
    {
        // Atualiza status de todos os personagens
        foreach (var cw in characterWaypoints)
        {
            if (cw.character && cw.waypoint)
                cw.isAtWaypoint = Vector3.Distance(cw.character.position, cw.waypoint.position) <= cw.activationDistance;
        }

        // Ativa fade do item assim que o PRIMEIRO personagem chegar
        if (!itemActivated && characterWaypoints.Count > 0 && characterWaypoints[0].isAtWaypoint)
        {
            StartCoroutine(FadeInItem());
            itemActivated = true;
        }

        // Ativa diálogo quando TODOS os personagens chegarem
        if (!dialogueActivated && characterWaypoints.TrueForAll(cw => cw.isAtWaypoint))
        {
            if (dialogueObject) dialogueObject.SetActive(true);
            dialogueActivated = true;
        }
    }

    IEnumerator FadeInItem()
    {
        if (!itemSprite) yield break;

        if (!itemSprite.gameObject.activeInHierarchy)
            itemSprite.gameObject.SetActive(true);

        Color color = itemSprite.color;
        while (color.a < 1f)
        {
            color.a = Mathf.MoveTowards(color.a, 1f, fadeSpeed * Time.deltaTime);
            itemSprite.color = color;
            yield return null;
        }
    }
}
