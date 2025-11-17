using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class DragAndDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Destino")]
    public Image destino;
    public float snapDist = 100f;
    public Image destinoImage;

    [Header("Manager")]
    public Guarita guarita;
    public AudioSource audioSource;

    [Header("Fade Inicial")]
    public float tempoFadeInicial = 1f;

    private Canvas canvas;
    private RectTransform selfRect;
    private Image selfImage;
    private CanvasGroup canvasGroup;

    private Vector3 startWorldPos;
    private bool segurando = false;
    private bool interacaoLiberada = false;
    private Coroutine piscarCoroutine;

    private static readonly Color32 DestinoBase = new Color32(0x67, 0x62, 0x62, 0xB4);
    private static readonly Color32 DestinoZero = new Color32(0x67, 0x62, 0x62, 0x00);

    private void Awake()
    {
        selfRect = GetComponent<RectTransform>();
        selfImage = GetComponent<Image>();
        audioSource = GetComponent<AudioSource>();
        canvas = GetComponentInParent<Canvas>();
        startWorldPos = selfRect.position;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Deixa sempre visível e clicável
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        interacaoLiberada = true;

        if (destinoImage == null) destinoImage = destino;
        if (destinoImage != null)
            destinoImage.color = DestinoZero;
    }


    private void Start()
    {
        StartCoroutine(FadeInInicial());
    }

    private IEnumerator FadeInInicial()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / tempoFadeInicial;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        interacaoLiberada = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!interacaoLiberada) return;

        segurando = true;

        if (destinoImage != null)
            piscarCoroutine = StartCoroutine(PiscarDestino());
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!interacaoLiberada) return;

        Vector2 delta = eventData.delta;
        if (canvas != null) delta /= canvas.scaleFactor;
        selfRect.position += new Vector3(delta.x, delta.y, 0f);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!interacaoLiberada) return;

        segurando = false;

        if (piscarCoroutine != null)
        {
            StopCoroutine(piscarCoroutine);
            piscarCoroutine = null;
        }

        if (destinoImage != null)
            destinoImage.color = DestinoZero;

        float dist = (destino != null)
            ? Vector2.Distance(selfRect.position, destino.rectTransform.position)
            : Mathf.Infinity;

        if (dist <= snapDist && destino != null)
        {
            selfRect.position = destino.rectTransform.position;
            
            if (destinoImage != null)
                destinoImage.color = Color.white;

            gameObject.SetActive(false);
            audioSource.Play();

            guarita.Ativar();
        }
        else
        {
            selfRect.position = startWorldPos;
        }
    }

    private IEnumerator PiscarDestino()
    {
        if (destinoImage == null) yield break;

        while (segurando)
        {
            destinoImage.color = DestinoBase;
            yield return new WaitForSeconds(0.5f);

            destinoImage.color = DestinoZero;
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void OnDrawGizmos()
    {
        if (destino != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(destino.transform.position, snapDist);
        }
    }
}
