using UnityEngine;

// Alvo de treino: pisca ao levar dano e some quando a vida acaba.
public class AlvoTreino : MonoBehaviour, IDanificavel
{
    [SerializeField] int vidaMaxima = 100;
    [SerializeField] Color corAtingido = Color.red;
    [SerializeField] float tempoPiscar = 0.1f;

    int vida;
    Renderer rend;
    Color corOriginal;
    float fimPiscar;
    bool piscando;

    void Awake()
    {
        vida = vidaMaxima;
        rend = GetComponentInChildren<Renderer>();
        corOriginal = rend.material.color;
    }

    public void ReceberDano(int dano, Vector3 origem)
    {
        if (dano <= 0) return;

        vida -= dano;
        Debug.Log($"{name} levou {dano} de dano. Vida: {vida}");

        rend.material.color = corAtingido;
        fimPiscar = Time.time + tempoPiscar;
        piscando = true;

        if (vida <= 0) Destroy(gameObject);
    }

    void Update()
    {
        if (piscando && Time.time >= fimPiscar)
        {
            rend.material.color = corOriginal;
            piscando = false;
        }
    }
}