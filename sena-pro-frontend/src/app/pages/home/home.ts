import { Component, OnInit, signal, WritableSignal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SenaProService } from '../../services/senapro.service';
import { SorteioStatus, AtualizacaoApiResultado, ImportacaoResultado } from '../../models/senapro.models';

@Component({
  selector: 'app-home',
  imports: [CommonModule],
  templateUrl: './home.html',
  styleUrl: './home.css',
  standalone: true
})
export class HomeComponent implements OnInit {
  status: WritableSignal<SorteioStatus | null> = signal(null);
  loadingStatus: WritableSignal<boolean> = signal(true);
  
  loadingApiUpdate: WritableSignal<boolean> = signal(false);
  apiResult: WritableSignal<AtualizacaoApiResultado | null> = signal(null);
  
  loadingImport: WritableSignal<boolean> = signal(false);
  importResult: WritableSignal<ImportacaoResultado | null> = signal(null);
  
  selectedFile: File | null = null;
  dragOver = false;

  constructor(private service: SenaProService) {}

  ngOnInit(): void {
    this.carregarStatus();
  }

  carregarStatus(): void {
    this.loadingStatus.set(true);
    this.service.getStatus().subscribe({
      next: (data) => {
        this.status.set(data);
        this.loadingStatus.set(false);
      },
      error: (err) => {
        console.error('Erro ao carregar status', err);
        this.loadingStatus.set(false);
      }
    });
  }

  atualizarApi(): void {
    this.loadingApiUpdate.set(true);
    this.apiResult.set(null);
    this.service.atualizarViaApi().subscribe({
      next: (res) => {
        this.apiResult.set(res);
        this.loadingApiUpdate.set(false);
        this.carregarStatus();
      },
      error: (err) => {
        this.apiResult.set({
          sucesso: false,
          mensagem: 'Falha na comunicação com o servidor.',
          novosSorteios: 0,
          haGap: false,
          quantidadeGap: 0,
          erros: [err?.error?.mensagem || 'Erro desconhecido.']
        });
        this.loadingApiUpdate.set(false);
      }
    });
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
      this.importResult.set(null);
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.dragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.dragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.dragOver = false;
    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      const file = files[0];
      const ext = file.name.split('.').pop()?.toLowerCase();
      if (ext === 'xlsx' || ext === 'xls') {
        this.selectedFile = file;
        this.importResult.set(null);
      } else {
        alert('Por favor, envie apenas arquivos Excel (.xlsx ou .xls).');
      }
    }
  }

  importarExcel(): void {
    if (!this.selectedFile) return;

    this.loadingImport.set(true);
    this.importResult.set(null);
    
    this.service.importarExcel(this.selectedFile).subscribe({
      next: (res) => {
        this.importResult.set(res);
        this.loadingImport.set(false);
        this.selectedFile = null;
        this.carregarStatus();
      },
      error: (err) => {
        const errorMsg = err?.error?.erros?.[0] || 'Erro desconhecido ao importar arquivo.';
        this.importResult.set({
          sucesso: false,
          mensagem: 'Falha ao importar o arquivo Excel.',
          registrosInseridos: 0,
          registrosIgnorados: 0,
          erros: [errorMsg]
        });
        this.loadingImport.set(false);
      }
    });
  }
}
