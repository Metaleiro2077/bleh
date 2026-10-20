using UnityEngine;
// Esse código é a classe que representa o estado ATTACK do jogador.
// Como ela herda de EstadoJogador, não usamos MonoBehaviour, e sim EstadoJogador como classe base.
// Essa é uma forma de implementar uma máquina de estados.
// onde cada estado é uma classe que herda de uma classe base
// e cada estado tem seus próprios métodos de entrada, atualização e saída.
public class EstadoATTACK : EstadoJogador
{
    public EstadoATTACK(JogadorControlador controlador) : base(controlador) { }

    // Estes campos sobrevivem de uma entrada para outra no estado,
    // porque o estado é criado UMA VEZ só, no Awake do controlador.
    int golpeAtual;             // 1, 2 ou 3
    float fimRecarga = -10f;    // quando o combo pode ser usado de novo

    // Dados do golpe em andamento
    float inicio;
    bool aplicouDano;
    bool pediuProximo;

    // Lido pelo controlador (PodeGolpear) para Idle e Walk decidirem se entram aqui
    public bool Disponivel => Time.time >= fimRecarga;

    float TempoNoGolpe => Time.time - inicio;
    bool EhFinalizador => golpeAtual >= controlador.GolpesNoCombo;

    public override void Entrar()
    {
        golpeAtual = 0;
        ComecarProximoGolpe();
    }

    // Reinicia o golpe sem sair do estado.
    // (TrocarEstado(Attack) aqui dentro não funcionaria: a guarda ignora troca para o mesmo estado.)
    void ComecarProximoGolpe()
    {
        golpeAtual++;
        inicio = Time.time;
        aplicouDano = false;
        pediuProximo = false;
    }

    public override void Atualizar()
    {
        // Clique durante o golpe: o jogador quer continuar o combo
        if (controlador.PediuGolpe) pediuProximo = true;

        if (TempoNoGolpe < controlador.DuracaoGolpe) return;

        if (pediuProximo && !EhFinalizador) controlador.TrocarEstado(controlador.Attack);

        // Combo completo: entra em recarga
        if (EhFinalizador)
            fimRecarga = Time.time + controlador.RecargaCombo;

        // Volta para o estado que combina com a situação
        if (!controlador.NoChao)
            controlador.TrocarEstado(controlador.Fall);
        else if (controlador.TemEntradaMover)
            controlador.TrocarEstado(controlador.Walk);
        else
            controlador.TrocarEstado(controlador.Idle);
    }

    public override void AtualizarFisica()
    {
        if (TempoNoGolpe < controlador.MomentoImpactoGolpe)
        {
            // Preparação: ainda dá para ajustar a mira, e o jogador avança
            controlador.GirarParaMira();
            controlador.Avancar();
        }
        else
        {
            // Depois do impacto: para, com a direção travada
            controlador.Frear();

            if (!aplicouDano)
            {
                controlador.AplicarGolpe(EhFinalizador);
            }
        }
    }
}