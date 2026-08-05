// === EPIC-001: Importação Excel ===
export interface ImportacaoResultado {
  sucesso: boolean;
  mensagem: string;
  registrosInseridos: number;
  registrosIgnorados: number;
  erros: string[];
}

// === EPIC-002: Sorteios Repetidos ===
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

// === EPIC-003: Gerador Inteligente de Jogos ===
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
