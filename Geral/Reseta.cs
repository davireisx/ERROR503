using UnityEngine;
using System.Collections;

public class ResetarPlayerPrefsNoFinal : MonoBehaviour
{
    public float tempoAntesDeApagar = 10f; // segundos

    void Start()
    {
        StartCoroutine(LimparDepoisDeDelay());
    }

    IEnumerator LimparDepoisDeDelay()
    {
        yield return new WaitForSeconds(tempoAntesDeApagar);
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("PlayerPrefs limpos após os créditos!");
    }
}
