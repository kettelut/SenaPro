import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AppService {
  private baseUrl = 'https://localhost:7235/api/MegaSena';

  constructor(private http: HttpClient) {}

  getResultados(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/ObterResultados`);
  }

  getUltimoResultado(): Observable<number[]> {
    return this.http.get<number[]>(`${this.baseUrl}/ObterUltimoResultado`);
  }

  getCincoNumerosMaisSorteados(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/ObterCincoNumerosMaisSorteados`);
  }

  getCincoNumerosMenosSorteados(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/ObterCincoNumerosMenosSorteados`);
  }

  getParesDeNumerosQueMaisAparecemJuntos(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/ParesDeNumerosQueMaisAparecemJuntos`);
  }

  getCalcularQntDeSorteiosAnterioresParaLocalizarNumeros(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/CalcularQntDeSorteiosAnterioresParaLocalizarNumeros`);
  }

  getObterSugetaoParaProximoSorteio(qntNumerosPorJogo: number, qntDeJogos: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/ObterSugetaoParaProximoSorteio?qntNumerosPorJogo=${qntNumerosPorJogo}&qntDeJogos=${qntDeJogos}`);
  }

  getObterSomasMaisSorteados(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/ObterSomasMaisSorteados`);
  }

}
