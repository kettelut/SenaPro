import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SenaProService {

  constructor(private http: HttpClient) { }

  calcular(valor: any, prazo: any) {
    const url = `${environment.urlApi}/api/Cdb/Calcular`;

    let queryParams = new HttpParams();
    queryParams = queryParams.append("valor", valor);
    queryParams = queryParams.append("meses", prazo);

    return this.http.get(url, { params: queryParams });
  }
}
