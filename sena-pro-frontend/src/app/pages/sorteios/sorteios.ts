import { Component, OnInit, signal, WritableSignal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SenaProService } from '../../services/senapro.service';
import { SorteiosRepetidosResultado, SorteioRepetidoInfo } from '../../models/senapro.models';

@Component({
  selector: 'app-sorteios',
  imports: [CommonModule],
  templateUrl: './sorteios.html',
  styleUrl: './sorteios.css',
  standalone: true
})
export class SorteiosComponent implements OnInit {
  // Lista de 1 a 60 para a grade de seleção
  numerosGrade: number[] = Array.from({ length: 60 }, (_, i) => i + 1);
  dezenasSelecionadas: WritableSignal<number[]> = signal([]);

  // Verificação de Jogo
  verificando = signal(false);
  verificou = signal(false);
  jaSorteado = signal(false);
  resultadoMsg = signal('');

  // Análise de Repetidos
  loadingRepetidos = signal(true);
  repetidosResultado: WritableSignal<SorteiosRepetidosResultado | null> = signal(null);

  constructor(private service: SenaProService) {}

  ngOnInit(): void {
    this.carregarRepetidos();
  }

  carregarRepetidos(): void {
    this.loadingRepetidos.set(true);
    this.service.getSorteiosRepetidos().subscribe({
      next: (res) => {
        this.repetidosResultado.set(res);
        this.loadingRepetidos.set(false);
      },
      error: (err) => {
        console.error('Erro ao carregar sorteios repetidos', err);
        this.loadingRepetidos.set(false);
      }
    });
  }

  toggleDezena(num: number): void {
    const selecionados = [...this.dezenasSelecionadas()];
    const index = selecionados.indexOf(num);

    if (index > -1) {
      // Remove se já selecionado
      selecionados.splice(index, 1);
      this.dezenasSelecionadas.set(selecionados);
      this.verificou.set(false); // Reseta resultado da busca
    } else {
      // Adiciona se não estourar o limite de 6
      if (selecionados.length < 6) {
        selecionados.push(num);
        selecionados.sort((a, b) => a - b);
        this.dezenasSelecionadas.set(selecionados);
        this.verificou.set(false); // Reseta resultado da busca
      }
    }
  }

  isSelecionado(num: number): boolean {
    return this.dezenasSelecionadas().includes(num);
  }

  limparSelecao(): void {
    this.dezenasSelecionadas.set([]);
    this.verificou.set(false);
  }

  verificarJogo(): void {
    const dezenas = this.dezenasSelecionadas();
    if (dezenas.length !== 6) return;

    this.verificando.set(true);
    this.verificou.set(false);

    this.service.verificarJogo(dezenas).subscribe({
      next: (res) => {
        this.jaSorteado.set(res.jaSorteado);
        this.verificou.set(true);
        this.verificando.set(false);
        if (res.jaSorteado) {
          this.resultadoMsg.set('Esta exata combinação de dezenas JÁ FOI sorteada em concursos passados da Mega-Sena!');
        } else {
          this.resultadoMsg.set('Combinação inédita! Estas dezenas nunca foram sorteadas juntas em nenhum concurso oficial.');
        }
      },
      error: (err) => {
        console.error('Erro ao verificar jogo', err);
        this.verificando.set(false);
        alert(err?.error?.mensagem || 'Erro ao processar verificação.');
      }
    });
  }
}
