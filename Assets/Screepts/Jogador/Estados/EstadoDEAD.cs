using UnityEngine;
// Esse código é a classe que representa o estado Dead do jogador.
// Como ela herda de EstadoJogador, não usamos MonoBehaviour, e sim EstadoJogador como classe base.
// Essa é uma forma de implementar uma máquina de estados.
// onde cada estado é uma classe que herda de uma classe base
// e cada estado tem seus próprios métodos de entrada, atualização e saída.
public class EstadoDEAD : EstadoJogador
{
    // Construtor que recebe o controlador do jogador para acessar suas propriedades e métodos.
    public EstadoDEAD(JogadorControlador controlador) : base(controlador) 
    {
    
    }
    public override void Entrar()
    {
        Debug.Log("Jogador morreu.");
        // Depois: avisar o GameManager para mostrar a tela de derrota
    }

    // Atualizar() fica vazio de propósito: Dead não tem saída.
    // Só sai reiniciando a fase.

    public override void AtualizarFisica()
    {
        // Para no lugar. Sem mira e sem movimento.
        controlador.Frear();
    }

}