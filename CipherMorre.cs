using UnityEngine;
using System.Collections;

public class VilaoMorte : MonoBehaviour
{
    [Header("Referência do Vilão")]
    public GameObject vilao;          // O vilão a ser desativado
    public GameObject dialogo;          // O vilão a ser desativado
    public Animator vilaoAnim;        // Animator do vilão

    private void OnEnable()
    {
        // Assim que este objeto for ativado, inicia a sequência de morte
        StartCoroutine(MorteDoVilao());
    }

    private IEnumerator MorteDoVilao()
    {
        if (vilaoAnim != null)
        {
            vilaoAnim.SetBool("morre", true);
        }

        yield return new WaitForSeconds(2f);

        if (vilao != null)
        {
            vilao.SetActive(false);
            dialogo.SetActive(true);
            
        }
    }
}
