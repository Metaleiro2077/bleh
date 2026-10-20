using UnityEngine;
// Esse código é a classe que representa o estado JUMP do jogador.
// Como ela herda de EstadoJogador, não usamos MonoBehaviour, e sim EstadoJogador como classe base.
// Essa é uma forma de implementar uma máquina de estados.
// onde cada estado é uma classe que herda de uma classe base
// e cada estado tem seus próprios métodos de entrada, atualização e saída.
public class EstadoJUMP : EstadoJogador
{
    public EstadoJUMP(JogadorControlador controlador) : base(controlador) { }

    public override void Entrar()
    {
        // Impulso único, no momento em que o estado começa
        controlador.Pular();
    }

    public override void Atualizar()
    {
        // Parou de subir (chegou no topo ou bateu a cabeça): começa a cair
        if (controlador.Rb.linearVelocity.y <= 0f)
            controlador.TrocarEstado(controlador.Fall);
    }

    public override void AtualizarFisica()
    {
        // Controle no ar durante a subida
        controlador.Mover();
        controlador.GirarParaMira();
    }
}