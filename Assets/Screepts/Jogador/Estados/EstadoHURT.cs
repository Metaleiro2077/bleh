using UnityEngine;
// Esse código é a classe que representa o estado Hurt do jogador.
// Como ela herda de EstadoJogador, não usamos MonoBehaviour, e sim EstadoJogador como classe base.
// Essa é uma forma de implementar uma máquina de estados.
// onde cada estado é uma classe que herda de uma classe base
// e cada estado tem seus próprios métodos de entrada, atualização e saída.
public class EstadoHURT : EstadoJogador
{
    // Construtor que recebe o controlador do jogador para acessar suas propriedades e métodos.
    public EstadoHURT(JogadorControlador controlador) : base(controlador) 
    {
    
    }

    // Momento em que o jogador recupera o controle
    float tempoSaida;

    public override void Entrar()
    {
        tempoSaida = Time.time + controlador.DuracaoHurt;
    }

    public override void Atualizar()
    {
        if (Time.time < tempoSaida) return;

        // Recuperou: vai para o estado que combina com a situação atual
        if (!controlador.NoChao)
            controlador.TrocarEstado(controlador.Fall);
        else if (controlador.TemEntradaMover)
            controlador.TrocarEstado(controlador.Walk);
        else
            controlador.TrocarEstado(controlador.Idle);
    }

    public override void AtualizarFisica()
    {
        // Sem Mover() e sem GirarParaMira(): o jogador perde o controle por um instante.
        // O empurrão aplicado em ReceberDano segue sozinho.
        controlador.Frear();
        controlador.AplicarGravidadeExtra();
    }

}