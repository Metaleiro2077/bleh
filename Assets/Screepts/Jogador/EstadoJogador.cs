using UnityEngine;
// Esse codigo é a classe base para todos os estados do jogador. 
// Por isso usamos o EstadoJogador como nome da classe, e ela é abstrata, para que não seja instanciada diretamente.
// Essa classe vai ter um construtor que recebe o controlador do jogador, para que os estados possam acessar o controlador e mudar de estado quando necessário.
public abstract class EstadoJogador
{

    protected JogadorControlador controlador;

    public EstadoJogador(JogadorControlador controlador)
    {
        this.controlador = controlador;
    }

    public virtual void Entrar() 
    {
    
    }
    public virtual void Atualizar() 
    {
    
    }
    public virtual void AtualizarFisica() 
    {
    
    }
    public virtual void Sair() 
    {
    
    }

}