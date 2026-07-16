import { Component, OnInit, signal, WritableSignal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SenaProService } from '../../services/senapro.service';
import { GeracaoJogosResultado, JogoSugerido } from '../../models/senapro.models';

@Component({
  selector: 'app-gerador',
  imports: [CommonModule, FormsModule],
  templateUrl: './gerador.html',
  styleUrl: './gerador.css',
  standalone: true
})
export class GeradorComponent implements OnInit {
  // Configurações padrão
  quantidadeNumeros = signal(6);
  quantidadeJogos = signal(5);
  evitarRepetidos = signal(true); // Controla o filtro SorteiosRepetidos

  // Opções permitidas
  opcoesDezenas: number[] = Array.from({ length: 10 }, (_, i) => i + 6); // 6 a 15 dezenas

  // Status de execução
  loadingAnalises = signal(true);
  gerando = signal(false);
  
  // Resultados
  analisesDisponiveis = signal<string[]>([]);
  resultado: WritableSignal<GeracaoJogosResultado | null> = signal(null);

  constructor(private service: SenaProService) {}

  ngOnInit(): void {
    this.carregarAnalises();
  }

  carregarAnalises(): void {
    this.loadingAnalises.set(true);
    this.service.getAnalisesDisponiveis().subscribe({
      next: (res) => {
        this.analisesDisponiveis.set(res);
        this.loadingAnalises.set(false);
      },
      error: (err) => {
        console.error('Erro ao obter análises disponíveis', err);
        this.loadingAnalises.set(false);
      }
    });
  }

  gerar(): void {
    this.gerando.set(true);
    this.resultado.set(null);

    const analises: string[] = [];
    if (this.evitarRepetidos() && this.analisesDisponiveis().includes('SorteiosRepetidos')) {
      analises.push('SorteiosRepetidos');
    }

    const config = {
      quantidadeNumeros: this.quantidadeNumeros(),
      quantidadeJogos: this.quantidadeJogos(),
      analisesRespeitadas: analises
    };

    this.service.gerarJogos(config).subscribe({
      next: (res) => {
        this.resultado.set(res);
        this.gerando.set(false);
      },
      error: (err) => {
        console.error('Erro ao gerar dezenas', err);
        this.gerando.set(false);
        const errorMsg = err?.error?.erros?.[0] || 'Erro inesperado ao gerar jogos.';
        alert(errorMsg);
      }
    });
  }

  copiarJogo(jogo: JogoSugerido): void {
    const texto = jogo.dezenas.map(d => String(d).padStart(2, '0')).join(' ');
    navigator.clipboard.writeText(texto).then(() => {
      // Pequeno alerta temporário (podemos fazer um badge dinâmico, mas um alert simples ou aviso local é suficiente)
      alert(`Jogo #${jogo.id} copiado para a área de transferência!`);
    });
  }

  copiarTodos(): void {
    const res = this.resultado();
    if (!res || res.jogos.length === 0) return;

    const texto = res.jogos
      .map(jogo => `Jogo #${jogo.id}: ${jogo.dezenas.map(d => String(d).padStart(2, '0')).join(' ')}`)
      .join('\n');

    navigator.clipboard.writeText(texto).then(() => {
      alert('Todos os jogos foram copiados com sucesso!');
    });
  }
}
