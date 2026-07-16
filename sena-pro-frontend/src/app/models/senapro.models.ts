export interface Sorteio {
  concurso: number;
  data: string;
  dezenas: number[];
  acumulado: boolean;
  premioSena?: number;
  ganhadoresSena?: number;
}

export interface SorteioStatus {
  totalSorteiosBanco: number;
  ultimoConcursoBanco?: number;
  ultimoConcursoApi?: number;
  haGap: boolean;
  quantidadeGap: number;
  ultimoSorteio?: Sorteio;
}

export interface ImportacaoResultado {
  sucesso: boolean;
  mensagem: string;
  registrosInseridos: number;
  registrosIgnorados: number;
  erros: string[];
}

export interface AtualizacaoApiResultado {
  sucesso: boolean;
  mensagem: string;
  ultimoConcursoBanco?: number;
  ultimoConcursoApi?: number;
  haGap: boolean;
  quantidadeGap: number;
  novosSorteios: number;
  erros: string[];
}

export interface SorteioRepetidoInfo {
  concurso1: number;
  data1: string;
  concurso2: number;
  data2: string;
  dezenas: number[];
}

export interface SorteiosRepetidosResultado {
  sucesso: boolean;
  mensagem: string;
  existemRepetidos: boolean;
  quantidadePares: number;
  pares: SorteioRepetidoInfo[];
  erros: string[];
}

export interface JogoSugerido {
  id: number;
  dezenas: number[];
  dataGeracao: string;
}

export interface ConfiguracaoGeracaoJogos {
  quantidadeNumeros: number;
  quantidadeJogos: number;
  analisesRespeitadas: string[];
}

export interface GeracaoJogosResultado {
  sucesso: boolean;
  mensagem: string;
  jogos: JogoSugerido[];
  quantidadeGerada: number;
  configuracao: ConfiguracaoGeracaoJogos;
  erros: string[];
}
