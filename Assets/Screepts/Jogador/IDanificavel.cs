using UnityEngine;

// Contrato: quem implementa esta interface pode levar dano.
// Golpes, projéteis e armadilhas procuram por ela, sem saber o tipo do alvo.
public interface IDanificavel
{
    void ReceberDano(int dano, Vector3 origem);
}