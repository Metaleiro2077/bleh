using UnityEngine;
// Esse código é a classe que representa o estado WALK do jogador.
// Como ela herda de EstadoJogador, não usamos MonoBehaviour, e sim EstadoJogador como classe base.
// Essa é uma forma de implementar uma máquina de estados.
// onde cada estado é uma classe que herda de uma classe base
// e cada estado tem seus próprios métodos de entrada, atualização e saída.
public class EstadoWALK : EstadoJogador
{
    public EstadoWALK(JogadorControlador controlador) : base(controlador) { }

    public override void Atualizar()
    {
        if (controlador.PodePular)
            controlador.TrocarEstado(controlador.Jump);
        else if (!controlador.NoChao)
            controlador.TrocarEstado(controlador.Fall);
        else if (controlador.PediuGolpe && controlador.PodeGolpear)                      
            controlador.TrocarEstado(controlador.Attack);
        else if (!controlador.TemEntradaMover)
            controlador.TrocarEstado(controlador.Idle);
    }

    public override void AtualizarFisica()
    {
        controlador.Mover();
        controlador.GirarParaMira();  
    }
}