using UnityEngine;
// Esse código é a classe que representa o estado FALL do jogador.
// Como ela herda de EstadoJogador, não usamos MonoBehaviour, e sim EstadoJogador como classe base.
// Essa é uma forma de implementar uma máquina de estados.
// onde cada estado é uma classe que herda de uma classe base
// e cada estado tem seus próprios métodos de entrada, atualização e saída.
public class EstadoFALL : EstadoJogador
{
    public EstadoFALL(JogadorControlador controlador) : base(controlador) { }

    public override void Atualizar()
    {
        // Coyote time: acabou de sair da beirada e apertou o pulo
        // Buffer: apertou o pulo pouco antes de pousar
        if (controlador.PodePular)
            controlador.TrocarEstado(controlador.Jump);
        else if (controlador.NoChao)
        {
            if (controlador.TemEntradaMover)
                controlador.TrocarEstado(controlador.Walk);
            else
                controlador.TrocarEstado(controlador.Idle);
        }
    }

    public override void AtualizarFisica()
    {
        controlador.Mover();
        controlador.GirarParaMira();  
    }
}