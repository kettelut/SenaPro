import { Component, OnInit } from '@angular/core';
import { AppService } from './app.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  resultados: any[] = [];
  resultadosPaginados: any[] = [];
  ultimoResultado: number[] = [];
  cincoNumerosMaisSorteados: any[] = [];
  cincoNumerosMenosSorteados: any[] = [];
  paresDeNumerosQueMaisAparecemJuntos: any[] = [];
  sorteiosAnterioresParaLocalizarNumeros: any[] = [];
  
  paginaAtual = 1;
  itensPorPagina = 20; // Quantidade de itens por página
  paginasVisiveis: number[] = [];
  totalPaginas: number = 0;
  title: string = 'senapro'; // Title
  activeTab: string = 'analise'; // Tab ativa inicial

  // Propriedades para o gerador de números
  qntNumerosPorJogo: number = 6;
  qntDeJogos: number = 1;
  qntNumerosdistintos: number = 0;
  numerosGerados: number[][] = [];

  loading: boolean = false; // Indicador de carregamento

  constructor(private appService: AppService) {}

  ngOnInit() {
    this.carregarDadosAnaliseSorteios();
    this.carregarTodosResultados();
  }

  setActiveTab(tab: string) {
    this.activeTab = tab;
  }

  carregarDadosAnaliseSorteios() {
    this.appService.getCincoNumerosMaisSorteados().subscribe(data => {
      this.cincoNumerosMaisSorteados = data;
    });

    this.appService.getCincoNumerosMenosSorteados().subscribe(data => {
      this.cincoNumerosMenosSorteados = data;
    });

    this.appService.getParesDeNumerosQueMaisAparecemJuntos().subscribe(data => {
      this.paresDeNumerosQueMaisAparecemJuntos = data;
    });

    this.appService.getCalcularQntDeSorteiosAnterioresParaLocalizarNumeros().subscribe(data => {
      this.sorteiosAnterioresParaLocalizarNumeros = data;
    });
  }

  carregarTodosResultados() {
    this.appService.getResultados().subscribe(data => {
      this.resultados = data.sort((a, b) => b.numeroConcurso - a.numeroConcurso); // Ordena os resultados em ordem decrescente
      this.totalPaginas = Math.ceil(this.resultados.length / this.itensPorPagina);
      this.atualizarPaginasVisiveis();
      this.irParaPagina(null, 1);
    });

    this.appService.getUltimoResultado().subscribe(data => {
      this.ultimoResultado = data;
    });
  }

  atualizarPaginasVisiveis() {
    const maxPaginasVisiveis = 5; // Quantidade máxima de páginas visíveis
    let startPage: number, endPage: number;

    if (this.totalPaginas <= maxPaginasVisiveis) {
      startPage = 1;
      endPage = this.totalPaginas;
    } else {
      if (this.paginaAtual <= Math.ceil(maxPaginasVisiveis / 2)) {
        startPage = 1;
        endPage = maxPaginasVisiveis;
      } else if (this.paginaAtual + Math.floor(maxPaginasVisiveis / 2) >= this.totalPaginas) {
        startPage = this.totalPaginas - maxPaginasVisiveis + 1;
        endPage = this.totalPaginas;
      } else {
        startPage = this.paginaAtual - Math.floor(maxPaginasVisiveis / 2);
        endPage = this.paginaAtual + Math.floor(maxPaginasVisiveis / 2);
      }
    }

    this.paginasVisiveis = Array.from({ length: (endPage + 1) - startPage }, (_, i) => startPage + i);
  }

  atualizarResultadosPaginados() {
    const startIndex = (this.paginaAtual - 1) * this.itensPorPagina;
    const endIndex = startIndex + this.itensPorPagina;
    this.resultadosPaginados = this.resultados.slice(startIndex, endIndex);
  }

  irParaPagina(event: any, pagina: number) {
    if (event) {
      event.preventDefault();
    }
    this.paginaAtual = pagina;
    this.atualizarPaginasVisiveis();
    this.atualizarResultadosPaginados();
  }

  paginaAnterior(event: any) {
    event.preventDefault();
    if (this.paginaAtual > 1) {
      this.paginaAtual--;
      this.atualizarPaginasVisiveis();
      this.atualizarResultadosPaginados();
    }
  }

  proximaPagina(event: any) {
    event.preventDefault();
    if (this.paginaAtual < this.totalPaginas) {
      this.paginaAtual++;
      this.atualizarPaginasVisiveis();
      this.atualizarResultadosPaginados();
    }
  }

  // Método para obter sugestão para o próximo sorteio
  obterSugestao() {
    this.loading = true; // Ativa o indicador de carregamento

    // Chamada assíncrona ao serviço para obter sugestão de números
    this.appService.getObterSugetaoParaProximoSorteio(this.qntNumerosPorJogo, this.qntDeJogos).subscribe(
      data => {
        this.numerosGerados = data;

        // Calcular a quantidade de números distintos
        const numerosDistintos = this.calcularNumerosDistintos(this.numerosGerados);

        // Atribuir a quantidade de números distintos à propriedade qntNumerosdistintos
        this.qntNumerosdistintos = numerosDistintos;

        this.loading = false; // Desativa o indicador de carregamento após recebimento dos dados
      },
      error => {
        this.loading = false; // Desativa o indicador de carregamento em caso de erro
        console.error('Erro ao obter sugestão de números:', error);
      }
    );
  }

  // Método para calcular a quantidade de números distintos
  calcularNumerosDistintos(numeros: number[][]): number {
    // Utilizar um conjunto (Set) para armazenar números únicos
    const numerosUnicos = new Set<number>();

    // Iterar sobre todos os jogos gerados
    numeros.forEach(jogo => {
      jogo.forEach(numero => {
        numerosUnicos.add(numero); // Adicionar cada número ao conjunto
      });
    });

    // Retornar o tamanho do conjunto, que representa a quantidade de números distintos
    return numerosUnicos.size;
  }
}
