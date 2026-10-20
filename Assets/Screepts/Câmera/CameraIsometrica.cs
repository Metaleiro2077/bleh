using UnityEngine;

// Câmera isométrica que segue o jogador.
// Fica na Main Camera, que NÃO deve ser filha do jogador.
public class CameraIsometrica : MonoBehaviour
{
    [Header("Alvo")]
    [SerializeField] Transform alvo;

    [Header("Enquadramento")]
    [Tooltip("X = inclinação para baixo, Y = giro em volta do jogador (graus).")]
    [SerializeField] Vector2 angulo = new Vector2(35f, 45f);
    [SerializeField] float distancia = 14f;
    [Tooltip("Ponto do jogador que a câmera mira, medido a partir do pé.")]
    [SerializeField] float alturaFoco = 1f;

    [Header("Suavização")]
    [Tooltip("Tempo aproximado para a câmera alcançar o jogador. 0 = colada.")]
    [SerializeField] float tempoSuavizacao = 0.15f;

    Vector3 velocidadeAtual;

    void Start()
    {
        // Começa já no lugar certo, sem "voar" até o jogador no primeiro segundo
        if (alvo != null) transform.position = CalcularPosicao();
    }

    void LateUpdate()
    {
        if (alvo == null) return;

        transform.position = Vector3.SmoothDamp(
            transform.position, CalcularPosicao(), ref velocidadeAtual, tempoSuavizacao);

        transform.rotation = Quaternion.Euler(angulo.x, angulo.y, 0f);
    }

    Vector3 CalcularPosicao()
    {
        Quaternion rotacao = Quaternion.Euler(angulo.x, angulo.y, 0f);
        Vector3 foco = alvo.position + Vector3.up * alturaFoco;

        // Anda "para trás" a partir do foco, na direção em que a câmera olha
        return foco - rotacao * Vector3.forward * distancia;
    }
}