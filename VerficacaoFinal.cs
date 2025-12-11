using UnityEngine;
using System.Collections.Generic;

public class WaypointVerifier : MonoBehaviour
{
    [System.Serializable]
    public class CharacterWaypoint
    {
        public Transform character;
        public Transform waypoint;
        public float activationDistance = 1.0f;
    }

    [Header("Configurações dos Personagens")]
    public List<CharacterWaypoint> characterWaypoints = new List<CharacterWaypoint>();

    [Header("Item para Ativar (Alpha)")]
    public SpriteRenderer itemSprite;
    public float fadeSpeed = 2.0f;

    [Header("Diálogo para Ativar")]
    public GameObject dialogueObject;

    private bool itemActivated = false;
    private bool dialogueActivated = false;

    void Update()
    {
        // Verificação do personagem principal no waypoint para ativar o item
        CheckMainCharacterWaypoint();

        // Verificação de todos os personagens nos waypoints para ativar diálogo
        CheckAllCharactersWaypoints();
    }

    void CheckMainCharacterWaypoint()
    {
        if (itemActivated || characterWaypoints.Count == 0) return;

        // Verifica apenas o primeiro personagem (personagem principal)
        CharacterWaypoint mainCharacter = characterWaypoints[0];

        if (IsCharacterAtWaypoint(mainCharacter))
        {
            StartCoroutine(FadeInItem());
            itemActivated = true;
        }
    }

    void CheckAllCharactersWaypoints()
    {
        if (dialogueActivated || characterWaypoints.Count == 0) return;

        // Verifica se TODOS os personagens estão nos seus waypoints
        bool allCharactersAtWaypoints = true;

        foreach (CharacterWaypoint cw in characterWaypoints)
        {
            if (!IsCharacterAtWaypoint(cw))
            {
                allCharactersAtWaypoints = false;
                break;
            }
        }

        if (allCharactersAtWaypoints)
        {
            ActivateDialogue();
            dialogueActivated = true;
        }
    }

    bool IsCharacterAtWaypoint(CharacterWaypoint cw)
    {
        if (cw.character == null || cw.waypoint == null)
        {
            Debug.LogWarning("Character ou Waypoint não atribuído!");
            return false;
        }

        float distance = Vector3.Distance(cw.character.position, cw.waypoint.position);
        return distance <= cw.activationDistance;
    }

    System.Collections.IEnumerator FadeInItem()
    {
        if (itemSprite == null) yield break;

        Color color = itemSprite.color;
        float targetAlpha = 1.0f;

        while (color.a < targetAlpha)
        {
            color.a = Mathf.MoveTowards(color.a, targetAlpha, fadeSpeed * Time.deltaTime);
            itemSprite.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        itemSprite.color = color;
    }

    void ActivateDialogue()
    {
        if (dialogueObject != null)
        {
            dialogueObject.SetActive(true);
            Debug.Log("Diálogo ativado! Todos os personagens estão nos waypoints.");
        }
    }

    // Método para debug visual no Editor
    void OnDrawGizmosSelected()
    {
        if (characterWaypoints == null) return;

        foreach (CharacterWaypoint cw in characterWaypoints)
        {
            if (cw.character != null && cw.waypoint != null)
            {
                Gizmos.color = IsCharacterAtWaypoint(cw) ? Color.green : Color.red;
                Gizmos.DrawLine(cw.character.position, cw.waypoint.position);
                Gizmos.DrawWireSphere(cw.waypoint.position, cw.activationDistance);
            }
        }
    }
}