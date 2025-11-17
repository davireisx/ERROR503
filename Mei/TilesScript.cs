using UnityEngine;
using UnityEngine.UI;

public class TilesScript : MonoBehaviour
{
    public Vector3 targetPosition;
    private Vector3 correctPosition;
    private Image image;
    public int number;
    public bool inRightPlace;

    // Cores em hexadecimal
    private Color correctColor;
    private Color wrongColor;

    void Awake()
    {
        correctPosition = transform.position;
        targetPosition = transform.position;
        image = GetComponent<Image>();

        // Define as cores hexadecimais
        ColorUtility.TryParseHtmlString("#8CCC92", out correctColor); // Verde
        ColorUtility.TryParseHtmlString("#BE9EE7", out wrongColor);  // Magenta
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, 0.5f);

        if (targetPosition == correctPosition)
        {
            image.color = correctColor;
            inRightPlace = true;
        }
        else
        {
            image.color = wrongColor;
            inRightPlace= false;
        }
    }
}