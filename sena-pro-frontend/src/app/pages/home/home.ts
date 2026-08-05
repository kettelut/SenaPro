import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SenaProService } from '../../services/senapro.service';
import { ImportacaoResultado } from '../../models/senapro.models';

@Component({
  selector: 'app-home',
  imports: [CommonModule],
  templateUrl: './home.html',
  styleUrl: './home.css',
  standalone: true
})
export class HomeComponent {
  loadingImport = false;
  importResult: ImportacaoResultado | null = null;

  selectedFile: File | null = null;
  dragOver = false;

  constructor(private service: SenaProService) {}

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
      this.importResult = null;
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
        this.importResult = null;
      } else {
        alert('Por favor, envie apenas arquivos Excel (.xlsx ou .xls).');
      }
    }
  }

  importarExcel(): void {
    if (!this.selectedFile) return;

    this.loadingImport = true;
    this.importResult = null;

    this.service.importarExcel(this.selectedFile).subscribe({
      next: (res) => {
        this.importResult = res;
        this.loadingImport = false;
        this.selectedFile = null;
      },
      error: (err) => {
        const errorMsg = err?.error?.erros?.[0] || 'Erro desconhecido ao importar arquivo.';
        this.importResult = {
          sucesso: false,
          mensagem: 'Falha ao importar o arquivo Excel.',
          registrosInseridos: 0,
          registrosIgnorados: 0,
          erros: [errorMsg]
        };
        this.loadingImport = false;
      }
    });
  }
}
