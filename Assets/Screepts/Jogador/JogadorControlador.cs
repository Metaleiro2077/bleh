using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
// Essa classe será responsável por controlar o jogador.
// Ela será responsável por controlar os estados do jogador
// E também será responsável por controlar a movimentação do jogador.
// Esse RequireComponent é necessário para garantir que o jogador tenha um Rigidbody
// Que é necessário para a movimentação do jogador.
[RequireComponent(typeof(Rigidbody))]
public class JogadorControlador : MonoBehaviour, IDanificavel
{
    // Aqui vamos colocar as referências para a câmera e para as variáveis de movimento do jogador.
    // Essas variáveis serão usadas para controlar a movimentação do jogador e poderão ser acessadas pelos estados do jogador.
    [Header("Referências")]
    [Tooltip("Câmera usada para orientar o movimento. Vazio = Camera.main.")]
    [SerializeField] Camera cameraJogo;

    [Header("Movimento")]
    [SerializeField] float velocidadeAndar = 6f;
    [SerializeField] float velocidadeCorrer = 9f;
    [Tooltip("Quão rápido o jogador chega na velocidade desejada (m/s²). Valores altos = resposta imediata.")]
    [SerializeField] float aceleracao = 60f;

    [Header("Checagem de chão")]
    [Tooltip("Quais camadas contam como chão.")]
    [SerializeField] LayerMask camadasChao = ~0;
    [Tooltip("Altura, a partir do pé, de onde a esfera de checagem sai.")]
    [SerializeField] float alturaChecagem = 0.5f;
    [SerializeField] float raioChecagem = 0.4f;
    [Tooltip("Quanto abaixo do pé a checagem alcança.")]
    [SerializeField] float folgaChao = 0.1f;

    [Header("Pulo")]
    [Tooltip("Altura máxima do pulo, em metros.")]
    [SerializeField] float alturaPulo = 1.4f;
    [Tooltip("Gravidade extra na descida, para o pulo não parecer flutuante.")]
    [SerializeField] float multiplicadorQueda = 2f;
    [Tooltip("Tempo em que ainda dá para pular depois de sair da beirada.")]
    [SerializeField] float tempoCoyote = 0.12f;
    [Tooltip("Tempo em que um pulo apertado um pouco antes de pousar ainda vale.")]
    [SerializeField] float tempoBufferPulo = 0.12f;

    [Header("Mira")]
    [Tooltip("Altura do plano onde o mouse é projetado (aprox. altura do peito).")]
    [SerializeField] float alturaMira = 1f;

    [Header("Vida")]
    [SerializeField] int vidaMaxima = 100;
    [Tooltip("Quanto tempo o jogador fica sem controle depois de levar dano.")]
    [SerializeField] float duracaoHurt = 0.4f;
    [Tooltip("Quanto tempo o jogador fica imune a novos danos.")]
    [SerializeField] float tempoInvulneravel = 1f;
    [Tooltip("Velocidade do empurrão ao levar dano.")]
    [SerializeField] float forcaEmpurrao = 6f;

    [Header("Golpe corpo a corpo")]
    [SerializeField] int danoGolpe = 25;
    [Tooltip("Dano do último golpe do combo.")]                                   
    [SerializeField] int danoFinalizador = 50;                                     
    [SerializeField] int golpesNoCombo = 3;                                        
    [Tooltip("Distância à frente do jogador onde fica o centro do golpe.")]
    [SerializeField] float alcanceGolpe = 1.2f;
    [SerializeField] float raioGolpe = 0.8f;
    [Tooltip("Duração de cada golpe, da preparação à recuperação.")]
    [SerializeField] float duracaoGolpe = 0.35f;
    [Tooltip("Momento, dentro do golpe, em que o dano acontece. Até lá, o jogador avança.")]
    [SerializeField] float momentoImpactoGolpe = 0.12f;
    [Tooltip("Velocidade do pequeno avanço durante a preparação do golpe.")]      
    [SerializeField] float velocidadeAvanco = 6f;                                 
    [Tooltip("Tempo sem poder golpear depois de completar o combo.")]             
    [SerializeField] float recargaCombo = 0.8f;                                    
    [Tooltip("Quais camadas o golpe pode atingir.")]
    [SerializeField] LayerMask camadasAtingiveis = ~0;

    // Vamos montar esse projeto como uma maquina de estados.
    // O jogador terá 7 estados que serão controlados por esse código
    // 1. Idle
    // 2. Walk
    // 3. Jump
    // 4. Fall
    // 5. Attack
    // 6. Hurt
    // 7. Dead
    // Esses estados serão controlados por uma máquina de estados que será implementada nesse código.

    // Vamos criar uma propriedade para o Rigidbody do jogador,
    // que será usada para controlar a movimentação do jogador.
    // Isso garante que a propriedade do Rb seja mantida em qualquer estado.
    public Rigidbody Rb { get; private set; }

    // Agora vamos colocar as entradas de InputAction
    // Essas entradas serão usadas para controlar a mudança de estado do jogador.
    // Todas elas levam o {get; private set;} para garantir que elas não sejam alteradas fora dessa classe.
    public Vector2 EntradaMover { get; private set; }
    public bool Correndo { get; private set; }
    public bool PediuPulo { get; private set; }
    public bool PediuGolpe { get; private set; }   // NOVO (combo): agora é o clique esquerdo

    // true quando o jogador está pisando em algo. Atualizado a cada passo de física.
    public bool NoChao { get; private set; }

    // Ponto no mundo para onde o mouse aponta.
    public Vector3 PontoMira { get; private set; }

    // Informações de vida que os estados e, depois, o HUD vão ler
    public int Vida => vida;
    public bool Invulneravel => Time.time < fimInvulnerabilidade;
    public float DuracaoHurt => duracaoHurt;

    // O estado Attack lê o ritmo do golpe e do combo
    public float DuracaoGolpe => duracaoGolpe;
    public float MomentoImpactoGolpe => momentoImpactoGolpe;
    public int GolpesNoCombo => golpesNoCombo;   // NOVO (combo)
    public float RecargaCombo => recargaCombo;   // NOVO (combo)

    // Referências para as ações do Input System.
    // São privadas: só o controlador conversa com o teclado.
    InputAction acaoMover;
    InputAction acaoPular;
    InputAction acaoCorrer;
    InputAction acaoAtacar;

    // Os 7 estados do jogador.
    // Públicos para leitura: um estado precisa enxergar os outros para pedir a troca.
    // Exemplo, dentro do Idle: controlador.TrocarEstado(controlador.Walk);
    public EstadoIDLE Idle { get; private set; }
    public EstadoWALK Walk { get; private set; }
    public EstadoJUMP Jump { get; private set; }
    public EstadoFALL Fall { get; private set; }
    public EstadoATTACK Attack { get; private set; }
    public EstadoHURT Hurt { get; private set; }
    public EstadoDEAD Dead { get; private set; }

    // O estado que está valendo agora. Só um por vez.
    EstadoJogador estadoAtual;

    // Momentos (em segundos de jogo) usados pelo coyote time e pelo buffer.
    // Começam em -10 para valerem "há muito tempo".
    float tempoUltimoChao = -10f;
    float tempoPedidoPulo = -10f;

    // Direção horizontal para onde o personagem deve olhar
    Vector3 direcaoOlhar;

    // Momento em que a invulnerabilidade acaba
    float fimInvulnerabilidade = -10f;

    // true quando o jogador está apertando alguma direção
    public bool TemEntradaMover => EntradaMover.sqrMagnitude > 0.01f;

    // true quando o pulo é permitido: esteve no chão há pouco E apertou o pulo há pouco
    public bool PodePular =>
        Time.time - tempoUltimoChao <= tempoCoyote &&
        Time.time - tempoPedidoPulo <= tempoBufferPulo;

    // NOVO (combo): o combo está fora da recarga? Quem sabe é o próprio estado Attack.
    public bool PodeGolpear => Attack.Disponivel;

    // Só para depuração: mostra no Inspector o nome do estado atual.
    [SerializeField] string nomeEstadoAtual;

    // Vida atual. Serializada para aparecer no Inspector durante os testes.
    [SerializeField] int vida;


    // Aqui no Awake vamos pegar o Rigidbody do jogador
    // Também vamos buscar as ações de InputAction que estão nas Project-wide Actions.
    // Porque no Awake e não no Start
    // Porque o Awake é chamado antes do Start
    // E queremos garantir que as ações estejam disponíveis antes de qualquer outro código tentar acessá-las.
    void Awake()
    {
        // 1. Pega o Rigidbody que está no mesmo objeto
        Rb = GetComponent<Rigidbody>();
        // Se ninguém arrastou uma câmera no Inspector, usa a câmera principal da cena
        if (cameraJogo == null) cameraJogo = Camera.main;

        // 2. Ajusta a física do jogador
        Rb.freezeRotation = true;
        Rb.interpolation = RigidbodyInterpolation.Interpolate;

        // 3. Busca as ações nas Project-wide Actions
        acaoMover = InputSystem.actions.FindAction("Move");
        acaoPular = InputSystem.actions.FindAction("Jump");
        acaoCorrer = InputSystem.actions.FindAction("Sprint");
        acaoAtacar = InputSystem.actions.FindAction("Attack");

        // 4. Avisa se alguma ação não foi encontrada
        if (acaoMover == null || acaoPular == null || acaoCorrer == null || acaoAtacar == null)
            Debug.LogWarning("JogadorControlador: alguma ação não foi encontrada nas Project-wide Actions.");

        // 5. Cria os 7 estados. Cada um recebe o controlador (this).
        Idle = new EstadoIDLE(this);
        Walk = new EstadoWALK(this);
        Jump = new EstadoJUMP(this);
        Fall = new EstadoFALL(this);
        Attack = new EstadoATTACK(this);
        Hurt = new EstadoHURT(this);
        Dead = new EstadoDEAD(this);

        // 6. Começa com a vida cheia
        vida = vidaMaxima;
    }

    // No LerEntrada vamos ler as entradas de InputAction e atualizar as propriedades do jogador.
    void LerEntrada()
    {
        // Valor contínuo: vale enquanto as teclas estão apertadas
        EntradaMover = acaoMover.ReadValue<Vector2>();

        // Segurando: true em todos os quadros enquanto o Shift estiver apertado
        Correndo = acaoCorrer.WasPressedThisFrame();

        // Toque: true só no quadro exato em que a tecla foi apertada
        PediuPulo = acaoPular.WasPressedThisFrame();
        PediuGolpe = acaoAtacar.WasPressedThisFrame();

        // Guarda QUANDO o pulo foi pedido (para o buffer)
        if (PediuPulo) tempoPedidoPulo = Time.time;
    }

    // Projeta o cursor num plano na altura do peito e guarda o ponto de mira
    void AtualizarMira()
    {
        if (Mouse.current == null || cameraJogo == null) return;

        Ray raio = cameraJogo.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane planoMira = new Plane(Vector3.up, new Vector3(0f, transform.position.y + alturaMira, 0f));

        if (planoMira.Raycast(raio, out float distancia))
        {
            PontoMira = raio.GetPoint(distancia);

            Vector3 paraMira = PontoMira - transform.position;
            paraMira.y = 0f;

            // Cursor em cima do personagem: mantém a última direção
            if (paraMira.sqrMagnitude > 0.01f)
                direcaoOlhar = paraMira.normalized;
        }
    }

    // Troca o estado ativo do jogador.
    // Único lugar do projeto que chama Sair() e Entrar().
    // Aqui é o coração da máquina de estados.
    // Vamos controlar a troca de estados do jogador, garantindo que o estado antigo saia e o novo entre.
    public void TrocarEstado(EstadoJogador novoEstado)
    {
        // 1. Se já está nesse estado, não faz nada
        if (novoEstado == estadoAtual) return;

        // 2. O estado antigo encerra o que estava fazendo
        estadoAtual?.Sair();

        // 3. Troca
        estadoAtual = novoEstado;
        nomeEstadoAtual = novoEstado.GetType().Name;

        // 4. O estado novo começa
        estadoAtual.Entrar();
    }

    // Porta de entrada de qualquer dano no jogador.
    // "origem" é de onde o dano veio, para empurrar o jogador para o lado oposto.
    public void ReceberDano(int dano, Vector3 origem)
    {
        // Morto não leva dano, invulnerável não leva dano, dano zero não conta
        if (estadoAtual == Dead || Invulneravel || dano <= 0) return;

        vida -= dano;
        fimInvulnerabilidade = Time.time + tempoInvulneravel;

        // Empurrão para longe da origem do dano
        Vector3 empurrao = transform.position - origem;
        empurrao.y = 0f;
        if (empurrao.sqrMagnitude > 0.001f)
            Rb.linearVelocity = empurrao.normalized * forcaEmpurrao + Vector3.up * Rb.linearVelocity.y;

        if (vida == 0) TrocarEstado(Dead);
        else
            TrocarEstado(Hurt);
    }

    // As funções abaixo são ferramentas de física que serão usadas pelos estados do jogador.
    // Ou seja, os estados do jogador não precisam saber como a física funciona, eles só chamam essas funções.
    // ---------- Ferramentas de física ----------
    // Chamadas pelos estados, dentro do AtualizarFisica().
    // Anda na direção do WASD, em relação à câmera
    public void Mover()
    {
        float velocidade = Correndo ? velocidadeCorrer : velocidadeAndar;
        Vector3 direcao = DirecaoRelativaACamera();
        AplicarVelocidadeHorizontal(direcao * velocidade);
    }

    // Desacelera até parar
    public void Frear()
    {
        AplicarVelocidadeHorizontal(Vector3.zero);
    }

    // Converte o WASD (2D) numa direção no chão (3D), de acordo com a câmera
    Vector3 DirecaoRelativaACamera()
    {
        Vector3 frente = Vector3.ProjectOnPlane(cameraJogo.transform.forward, Vector3.up).normalized;
        Vector3 direita = Vector3.ProjectOnPlane(cameraJogo.transform.right, Vector3.up).normalized;

        Vector3 direcao = direita * EntradaMover.x + frente * EntradaMover.y;
        return Vector3.ClampMagnitude(direcao, 1f);
        return direcao;
    }

    // Leva a velocidade horizontal até a desejada, sem mexer na vertical
    void AplicarVelocidadeHorizontal(Vector3 desejada)
    {
        Vector3 atual = Rb.linearVelocity;
        Vector3 horizontal = new Vector3(atual.x, 0f, atual.z);

        horizontal = Vector3.MoveTowards(horizontal, desejada, aceleracao * Time.fixedDeltaTime);

        Rb.linearVelocity = new Vector3(horizontal.x, atual.y, horizontal.z);
    }

    // Verifica se há chão logo abaixo do pé, soltando uma esfera de dentro da cápsula
    void ChecarChao()
    {
        Vector3 origem = transform.position + Vector3.up * alturaChecagem;
        float distancia = (alturaChecagem - raioChecagem) + folgaChao;

        bool tocando = Physics.SphereCast(origem, raioChecagem, Vector3.down, out _, distancia,
                                          camadasChao, QueryTriggerInteraction.Ignore);

        // Subindo rápido (logo depois do pulo) não conta como chão
        NoChao = tocando && Rb.linearVelocity.y <= 0.5f;

        // Guarda QUANDO esteve no chão pela última vez (para o coyote time)
        if (NoChao) tempoUltimoChao = Time.time;
    }

    // Impulso para cima calculado para atingir exatamente a alturaPulo
    public void Pular()
    {
        // v = raiz(2 · g · h)
        float velocidadePulo = Mathf.Sqrt(2f * -Physics.gravity.y * alturaPulo);
        Rb.linearVelocity = new Vector3(Rb.linearVelocity.x, velocidadePulo, Rb.linearVelocity.z);

        // Consome o pedido e o coyote time, para não pular duas vezes
        tempoPedidoPulo = -10f;
        tempoUltimoChao = -10f;
    }

    // Puxa mais forte para baixo enquanto desce
    public void AplicarGravidadeExtra()
    {
        if (Rb.linearVelocity.y < 0f)
            Rb.AddForce(Physics.gravity * (multiplicadorQueda - 1f), ForceMode.Acceleration);
    }

    // Gira o personagem para a mira. Chamada pelos estados que permitem mirar.
    public void GirarParaMira()
    {
        if (direcaoOlhar.sqrMagnitude < 0.001f) return;
        Rb.MoveRotation(Quaternion.LookRotation(direcaoOlhar, Vector3.up));
    }

    // Combo: pequeno avanço na direção da mira, durante a preparação do golpe
    public void Avancar()
    {
        Vector3 direcao = direcaoOlhar.sqrMagnitude > 0.001f ? direcaoOlhar : transform.forward;
        Vector3 velocidade = direcao * velocidadeAvanco;

        // Mexe só no horizontal: a gravidade continua agindo
        Rb.linearVelocity = new Vector3(velocidade.x, Rb.linearVelocity.y, velocidade.z);
    }

    // Causa dano em tudo que for IDanificavel dentro da esfera à frente.
    // Combo: o finalizador (último golpe do combo) causa mais dano.
    public void AplicarGolpe(bool finalizador)
    {
        int dano = finalizador ? danoFinalizador : danoGolpe;

        Collider[] atingidos = Physics.OverlapSphere(CentroGolpe(), raioGolpe,
                                                     camadasAtingiveis, QueryTriggerInteraction.Ignore);

        // Um mesmo alvo pode ter vários colisores: cada alvo leva o golpe uma vez só
        var jaAtingidos = new HashSet<IDanificavel>();

        foreach (Collider col in atingidos)
        {
            IDanificavel alvo = col.GetComponentInParent<IDanificavel>();
            if (alvo == null || ReferenceEquals(alvo, this) || jaAtingidos.Contains(alvo)) continue;

            jaAtingidos.Add(alvo);
            alvo.ReceberDano(dano, transform.position);
        }
    }

    // Centro da esfera do golpe, na altura do peito, à frente do jogador
    Vector3 CentroGolpe()
    {
        return transform.position + Vector3.up * alturaMira + transform.forward * alcanceGolpe;
    }

    // Aqui depois do Awake, no Start, vamos colocar o jogador no estado Idle.
    void Start()
    {
        // O jogador começa parado
        TrocarEstado(Idle);
    }

    // No Update vamos ler o teclado e atualizar o estado ativo do jogador.
    void Update()
    {
        // 1. Primeiro lê o teclado
        LerEntrada();

        // 2. Depois calcula para onde o mouse aponta
        AtualizarMira();

        // 3. Depois o estado ativo decide o que fazer com essa entrada
        estadoAtual.Atualizar();

        // TESTE TEMPORÁRIO: K causa 25 de dano vindo da frente. Apagar quando houver inimigos.
        if (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
            ReceberDano(25, transform.position + transform.forward);
    }

    // O FixedUpdate é chamado a cada quadro de física.
    // que é diferente do Update, que é chamado a cada quadro de renderização.
    // Ele é usado para aplicar a física do jogador, como velocidade e forças.
    // Assim garantimos que os cálculos de física sejam feitos de forma consistente
    // independente da taxa de quadros do jogo.
    void FixedUpdate()
    {
        // 1. Primeiro descobre se está no chão
        ChecarChao();

        // 2. Depois o estado ativo aplica a física (velocidade, forças)
        estadoAtual.AtualizarFisica();
    }

    // Desenha os auxiliares na janela Scene quando o jogador está selecionado. Não entram na build.
    void OnDrawGizmosSelected()
    {
        // Checagem de chão: verde = no chão, vermelho = no ar
        Vector3 origem = transform.position + Vector3.up * alturaChecagem;
        float distancia = (alturaChecagem - raioChecagem) + folgaChao;

        Gizmos.color = NoChao ? Color.green : Color.red;
        Gizmos.DrawWireSphere(origem + Vector3.down * distancia, raioChecagem);

        // Alcance do golpe (laranja)
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(CentroGolpe(), raioGolpe);

        // Linha até o ponto de mira (amarelo)
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position + Vector3.up * alturaMira, PontoMira);
            Gizmos.DrawWireSphere(PontoMira, 0.2f);
        }
    }
}