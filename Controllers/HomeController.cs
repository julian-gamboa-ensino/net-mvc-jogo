using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using jogo_labirinto.Models;
using System.IO;
using Microsoft.AspNetCore.Http;


namespace jogo_labirinto.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        string saida="";

        public IActionResult Saida(string arquivoEntrada, string caminhoSAIDA)
        {
            ViewData["arquivoEntrada"]=arquivoEntrada;
            ViewData["caminhoSAIDA"]=caminhoSAIDA;
            return View();
        }

        public IActionResult Erro()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
/********************************************************
Julian Gamboa (https://www.linkedin.com/in/julian-gamboa-bahia/)

Desafio Técnico Empresa "CAPITAL ATOS". 
Este algoritmo foi implementado de forma didática em: 

    https://replit.com/@JulianJunho2020/agosto05#main.cs

Conceitos básicos usados para desenhar este algoritmo:

1) Matriz de conetividade:  
    Matriz onde cada elemento representa uma posição do labirinto, cujo valor indica
    o número de nodes com os quais se comunica o node da posição.
2) Matriz de visitas:
    Possui as mesmas dimensões que a matriz que representa o "labirinto estudado". 

    Inicialmente cada elemento desta matriz tem o valor de false dado que nennhum espaço 
    do labirinto foi percorrido pelo caminhante. Já com o decorrer da caminhada está matriz muda
    e cada posição caminhada será representada com um novo valor true desta matriz.

3) node (ponto da matriz): se considera um node (de um grafo G) um elemento da matriz

*********************************************************/

        int labirinto_filas; //Dimensão da matriz que está sendo usada para representar o Labirinto
        int labirinto_colunas;//Dimensão da matriz...
        string[] labirinto_dimensoes;// A primeira linha do arquivo de entrada deve conter as dimensões
        char[,] labirinto_matriz;//Refinamento:no caso que o arquivo de entrada não tenha indicada um ponto de entrada 
        bool matriz_conetividade_dada_porta_entrada=false;//Refinamento:no caso que o arquivo de entrada não tenha indicada um ponto de entrada 
        bool matriz_conetividade_dada_porta_saida=false;//Refinamento:no caso que o arquivo de entrada não tenha indicada um ponto de saída
        int[,] labirinto_matriz_conetividade;// que será usado para construir uma "Matriz de conetividade"
        bool[,] labirinto_matriz_visitados;// que será usado para construir uma "Matriz de VISITADOS"
        int[] labirinto_ponto_entrada=new int[2];// Indicação explicita do ponto de entrada (Par Cartesiano)

/****************************************************************
Nesta função inicia-se um conjunto de atividades de forma cuidadosa:
1) Lendo as dimensões do labirinto.
2) Colocando as informações (lidas do arquivo de entrada) na matriz "labirinto_matriz"
3) Construir uma matriz de conetividade, de visitados, 
4) Fazer a caminhada pelo labirinto
*****************************************************************/        
[HttpPost]
public ActionResult Upload(IFormFile arquivo) 
{
    //verifica se existem arquivos 
    if (arquivo == null || arquivo.Length == 0)
    {
        //ViewData["Erro"] = "Error: Arquivo(s) não selecionado(s)";
        return RedirectToAction("Erro");
    }
    

    if (arquivo.FileName.Contains(".txt"))
    {
        string[] lines;

        using (var reader = new StreamReader(arquivo.OpenReadStream()))
        {
            reader.Peek();
            string primeira_linha=reader.ReadLine();
//1) Lendo as dimensões do labirinto:
//A primeira linha do arquivo de entrada deve conter as dimensões no formato : "L C"
            labirinto_dimensoes = primeira_linha.Split(' '); 

            labirinto_filas=Convert.ToInt32(labirinto_dimensoes[0]);

            labirinto_colunas=Convert.ToInt32(labirinto_dimensoes[1]);

//2) Colocando as informações (lidas do arquivo de entrada):

            labirinto_matriz=new char[labirinto_filas,labirinto_colunas];

            lines=new string[labirinto_filas+1];
            int posicao_leitura=0;

            lines[0]=primeira_linha;

            while(reader.Peek()>=0)
            {
                lines[++posicao_leitura]=reader.ReadLine();
//Console.WriteLine(lines[posicao_leitura]);
            }

//Sabendo as dimesões
            for (int indice_filha = 0; indice_filha < labirinto_filas; indice_filha++) 
            {
                for (int indice_coluna = 0; indice_coluna < labirinto_colunas; indice_coluna++) 
                {
//Dado que a linha inicial contem informações de dimensões usa-se o acrecimo "lines[indice_filha+1]"
                    labirinto_matriz[indice_filha,indice_coluna]=Convert.ToChar(lines[indice_filha+1].Split(' ')[indice_coluna]);

//Refinamento: No caso de ter um ponto de entrada , será construida a matriz de conetividade

                    if(labirinto_matriz[indice_filha,indice_coluna]==Convert.ToChar("X"))
                    {
                        matriz_conetividade_dada_porta_entrada=true;
                    }
    //Verifica-se se o labirinto tem ponto de saída
                    if(
                        (indice_filha==0) 
                        || 
                        (indice_coluna==0)
                        ||  
                        (indice_filha==(labirinto_filas-1))
                        || 
                        (indice_coluna==(labirinto_colunas-1))
                    )
                    {
                        if(labirinto_matriz[indice_filha,indice_coluna]==Convert.ToChar("0"))
                        {
//No caso de ter um ponto de saida , será construida a matriz de conetividade
                            matriz_conetividade_dada_porta_saida=true;
                        }
                    }
                }  
            }
//O labirinto será estudado apenas se tiver "ponto de ENTRADA" e "ponto de SAIDA", caso no qual será:
//3) Construir uma matriz de conetividade, de visitados, 
//4) Fazer a caminhada pelo labirinto
            if(
                matriz_conetividade_dada_porta_entrada
                &&
                matriz_conetividade_dada_porta_saida
            )
            {
                Construir_Matriz_Conetividade();
                Construir_Matriz_visitados();
                Construir_Caminho_Laberinto();
//Console.WriteLine("//4) Fazer a caminhada pelo labirinto"); 
            }
//////////////////////
        }


    }
    else
    {
//Console.WriteLine("Arquivo incorrecto ");  

    }        

    return(RedirectToAction("Saida", new { 
        arquivoEntrada = arquivo.FileName ,
        caminhoSAIDA= this.saida ,
        }));

}
///////
/**********************************************************************
Construir a matriz de conetividade
Função elemental para 
    criar uma matriz cujos valores são todos false.
    preencher dita matriz considerando:
    1) O estudo de conetividade de cada node contando os nodes que lhe rodeian nas direções ortogonais i,j
    2) Colocando o valor de zero (nada conetado) para aquele node (ponto da matriz), que seja "parede"

**********************************************************************/
        public void Construir_Matriz_Conetividade () {

            int rowLength = labirinto_matriz.GetLength(0);
            int colLength = labirinto_matriz.GetLength(1);

            labirinto_matriz_conetividade=new int[rowLength,colLength];

            for (int i = 0; i < rowLength; i++)
            {
                for (int j = 0; j < colLength; j++)
                {
                    if(
                        (labirinto_matriz[i, j]==Convert.ToChar("0")) 
                        ||  
                        (labirinto_matriz[i, j]==Convert.ToChar("X"))
                        )
                    {                
//Refinameto: No caso de ter um ponto de partida ele vai iniciar uma SAIDA

                        if(labirinto_matriz[i, j]==Convert.ToChar("X"))
                        {
//O ponto de entra , inicialmente registrado como "X", será colocado como mais um elemento do labirinto ("0")
                            labirinto_matriz[i, j]=Convert.ToChar("0");
                            labirinto_ponto_entrada[0]=i;
                            labirinto_ponto_entrada[1]=j;
                            //Console.WriteLine(String.Format("O [{0}, {1}]",i+1,j+1));
                        }
//1) O estudo de conetividade de cada node contando os nodes que lhe rodeian nas direções ortogonais i,j
                        int node_conetividade=0;
                        try{
                            if(labirinto_matriz[i-1, j]==Convert.ToChar("0")) node_conetividade++;
                        } catch (IndexOutOfRangeException )  {}
                        try{
                            if(labirinto_matriz[i+1, j]==Convert.ToChar("0")) node_conetividade++;
                        } catch (IndexOutOfRangeException )  {}
                        try{
                            if(labirinto_matriz[i, j-1]==Convert.ToChar("0")) node_conetividade++;
                        } catch (IndexOutOfRangeException )  {}
                        try{
                            if(labirinto_matriz[i, j+1]==Convert.ToChar("0")) node_conetividade++;
                        } catch (IndexOutOfRangeException )  {}

                        labirinto_matriz_conetividade[i, j]=node_conetividade;

                    }
                    else
                    {
//2) Colocando o valor de zero (nada conetado) para aquele node (ponto da matriz), que seja "parede"                        
                        //Console.Write("*");
                        labirinto_matriz_conetividade[i, j]=0;
                    }
                        
                    }
                    //Console.Write(Environment.NewLine + Environment.NewLine);
                }
        }  
////    
/**********************************************************************
Construir a matriz de visitados:
Função elemental para criar uma matriz cujos valores são todos false.
**********************************************************************/
        public void Construir_Matriz_visitados () {
            int rowLength = labirinto_matriz.GetLength(0);
            int colLength = labirinto_matriz.GetLength(1);

            labirinto_matriz_visitados=new bool[rowLength,colLength];

            for (int i = 0; i < rowLength; i++)
            {
                for (int j = 0; j < colLength; j++)
                {
                    labirinto_matriz_visitados[i,j]=false;
                }
            }
        }   
 
/**********************************************************************
Construindo o Caminho aplicando as regras:

Lembremos que o algoritmo foi desenhado considerando que uma pessoa "caminhante" tinha que:

1) Se colocar no ponto de partida "O"
2) No caso de ter escolhido um caminho sem saida (travamento) deverá requar até achar 
    um ponto onde possa dar um passo nunca dado.
3) Analisar alternativas de caminho, desde uma dada posição
4) Para cada passo, verifica-se que não um ponto de saída, caso no qual acaba este estudo


**********************************************************************/
        public void Construir_Caminho_Laberinto()
        {
//Cada vez que se estuda um ponto do labirinto será usada uma pilha de alternativas            
            Stack stack_alternativas = new Stack(); 
//Os passos dados (direções i,j) podem se colocar numa pilha, para facilitar um eventual recuo do "caminhante"
            Stack stack_passo_i = new Stack();            
            Stack stack_passo_j = new Stack();
//Son registradas (direções i,j) las posiciones de los pasos POSSÌVEIS do "caminhante"
            Stack stack_posicao_i = new Stack();
            Stack stack_posicao_j = new Stack();
//Usa-se uma fila (estrutura de dados do tipo FIFO) para escrever o arquivo de saída
            Queue<string> Queue_passos_saida_arquivo = new Queue<string>();

//Caminhante: 1) Se colocar no ponto de partida "O"
            stack_alternativas.Push("O ["+(labirinto_ponto_entrada[0]+1)+", "+(labirinto_ponto_entrada[1]+1)+"]");
            stack_posicao_i.Push(labirinto_ponto_entrada[0]);
            stack_posicao_j.Push(labirinto_ponto_entrada[1]);

            
//Variavéis auxiliares que serão usadas no caso de "travamento"      
//2) No caso de ter escolhido um caminho sem saida (travamento) deverá requar até achar        
            int estudado=0;
            int i=0;
            int j=0;
            int reposiciona_i=0;
            int reposiciona_j=0;
            int travado_i=0;
            int travado_j=0;
            string movimento="";

//Será usado um LOOP da forma "do...while" dado que sempre vai se ter um ponto de entrada "X" , o que 
// justifica o uso de um "controle de loop" avaliado ao final do bloco

            do{

                if(stack_alternativas.Count>0)
                {
//Na primeira passada neste LOOP registra-se no arquivo de saída o ponto de entrada

                    Queue_passos_saida_arquivo.Enqueue(""+stack_alternativas.Pop());

                    i=Convert.ToInt32(stack_posicao_i.Pop());
                    j=Convert.ToInt32(stack_posicao_j.Pop());

                    stack_passo_i.Push(i);
                    stack_passo_j.Push(j);

//Dado que já foi visitada a posição (i,j) será registrada tal visita na matriz "labirinto_matriz_visitados"
                    
                    labirinto_matriz_visitados[i,j]=true;

//Dado que o caminhante já deu seu passo, e por tanto não precisa de mais alternativas, zeramos as alternativas
                    stack_alternativas.Clear();
                    stack_posicao_i.Clear();
                    stack_posicao_j.Clear();                    
                }

//3) Analisar alternativas de caminho, desde uma dada posição, lembrando a tabela de prioridades dada:
//prioridad de ordem "4" Ir para baixo (B)
                try{
                    estudado=labirinto_matriz_conetividade[i+1, j];
                    if((estudado>0) && (!labirinto_matriz_visitados[i+1,j]))
                    {
                    stack_alternativas.Push("B "+"["+(i+2)+", "+(j+1)+"]");
                    stack_posicao_i.Push(i+1);
                    stack_posicao_j.Push(j);
                    }
                } catch (IndexOutOfRangeException )  {}
//prioridad de ordem "3" Ir para a direita (D)
                try{
                    estudado=labirinto_matriz_conetividade[i, j+1];
                    if((estudado>0) && (!labirinto_matriz_visitados[i,j+1]))
                    {
                    stack_alternativas.Push("D "+"["+(i+1)+", "+(j+2)+"]");
                    stack_posicao_i.Push(i);
                    stack_posicao_j.Push(j+1);
                    }
                } catch (IndexOutOfRangeException )  {}
//prioridad de ordem "2" Ir para a esquerda (E)
                try{
                    estudado=labirinto_matriz_conetividade[i, j-1];
                    if((estudado>0) && (!labirinto_matriz_visitados[i,j-1]))
                    {
                    stack_alternativas.Push("E "+"["+(i+1)+", "+(j)+"]");
                    stack_posicao_i.Push(i);
                    stack_posicao_j.Push(j-1);
                    }
                } catch (IndexOutOfRangeException )  {}
//prioridad de ordem "1"  Ir para cima (C)
                try{
                    estudado=labirinto_matriz_conetividade[i-1, j];
                    if((estudado>0) && (!labirinto_matriz_visitados[i-1,j]))
                    {
                    stack_alternativas.Push("C "+"["+i+", "+(j+1)+"]");
                    stack_posicao_i.Push(i-1);
                    stack_posicao_j.Push(j);
                    }
                } catch (IndexOutOfRangeException )  {}

/* No caso que esteja num nodo "travado", ele pode visitar de novo o passo anterior (algo assim como um recuar um 1 passo) 
. o caminhante deve recuar quando sua pilha de alternativas esteja vazia*/

                if(stack_posicao_j.Count==0)
                {
//Coordenadas da posição onde Travou (que foi o passo registrado "momentos antes")
                    travado_i=Convert.ToInt32(stack_passo_i.Pop());
                    travado_j=Convert.ToInt32(stack_passo_j.Pop());

//Coordenadas da posição anterior que será o passo que o "caminhante" deve dar para recuar 
                    reposiciona_i=Convert.ToInt32(stack_passo_i.Pop());
                    reposiciona_j=Convert.ToInt32(stack_passo_j.Pop());

    //Será estudado o passo de recuo, para efeitos de respeitar o formato de saída, vendo as coordenadas
                    if((travado_i-reposiciona_i)==-1)
                    {
                        movimento="B";
                    }
                    if((travado_i-reposiciona_i)==1)
                    {
                        movimento="C";
                    }
                    if((travado_j-reposiciona_j)==-1)
                    {
                        movimento="D";
                    }
                    if((travado_j-reposiciona_j)==1)
                    {
                        movimento="E";
                    }

                    stack_alternativas.Push(movimento+" "+"["+(reposiciona_i+1)+", "+(reposiciona_j+1)+"]");

//finalza-le o "destravamento" colocando as posições onde será recolocado o "caminhante"
                    stack_posicao_i.Push(reposiciona_i);
                    stack_posicao_j.Push(reposiciona_j);
                }

//4) Para cada passo, verifica-se que não um ponto de saída, caso no qual acaba este estudo
                if(
                    (Convert.ToInt32(stack_posicao_i.Peek())==0)
                    ||
                    (Convert.ToInt32(stack_posicao_j.Peek())==0)
                    ||
                    (Convert.ToInt32(stack_posicao_i.Peek())==(labirinto_matriz.GetLength(0)-1))
                    ||
                    (Convert.ToInt32(stack_posicao_j.Peek())==(labirinto_matriz.GetLength(1)-1))
                )
                {
                    Queue_passos_saida_arquivo.Enqueue(""+stack_alternativas.Pop());
                    break;
                }


            } while (true);
//Colocando a saída no arquivo 
                    
            //using (StreamWriter sw = File.CreateText(path))
            {
                foreach( string passo in Queue_passos_saida_arquivo )
                {
//Console.WriteLine(passo);
                    this.saida=this.saida+"<br>"+passo;
                    //sw.WriteLine(passo);
                }                
            }
//Console.WriteLine(saida);
	

        }



//////////////////////////////////////
    }
}
