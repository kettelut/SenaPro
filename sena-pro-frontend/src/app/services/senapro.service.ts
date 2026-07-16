import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  SorteioStatus,
  AtualizacaoApiResultado,
  ImportacaoResultado,
  SorteiosRepetidosResultado,
  ConfiguracaoGeracaoJogos,
  GeracaoJogosResultado
} from '../models/senapro.models';

@Injectable({
  providedIn: 'root'
})
export class SenaProService {
  private apiUrl = 'http://localhost:5000/api';

  constructor(private http: HttpClient) {}

  getStatus(): Observable<SorteioStatus> {
    return this.http.get<SorteioStatus>(`${this.apiUrl}/sorteios/status`);
  }

  atualizarViaApi(): Observable<AtualizacaoApiResultado> {
    return this.http.post<AtualizacaoApiResultado>(`${this.apiUrl}/sorteios/atualizar-api`, {});
  }

  importarExcel(file: File): Observable<ImportacaoResultado> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ImportacaoResultado>(`${this.apiUrl}/sorteios/importar-excel`, formData);
  }

  getSorteiosRepetidos(): Observable<SorteiosRepetidosResultado> {
    return this.http.get<SorteiosRepetidosResultado>(`${this.apiUrl}/sorteios/repetidos`);
  }

  verificarJogo(dezenas: number[]): Observable<{ jaSorteado: boolean }> {
    return this.http.post<{ jaSorteado: boolean }>(`${this.apiUrl}/sorteios/verificar`, dezenas);
  }

  getAnalisesDisponiveis(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/gerador/analises`);
  }

  gerarJogos(config: ConfiguracaoGeracaoJogos): Observable<GeracaoJogosResultado> {
    return this.http.post<GeracaoJogosResultado>(`${this.apiUrl}/gerador/gerar`, config);
  }
}
