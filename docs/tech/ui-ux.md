# Documentação de UI/UX — SenaPro

Documento que descreve o sistema de design, padrões de interface e decisões de UX aplicados na aplicação **SenaPro** — plataforma de análise estatística e geração inteligente de jogos para loterias.

---

## Índice

1. [Visão Geral do Design](#visão-geral-do-design)
2. [Design Tokens](#design-tokens)
3. [Layout & Estrutura da Aplicação](#layout--estrutura-da-aplicação)
4. [Páginas e Navegação](#páginas-e-navegação)
5. [Componentes UI Reutilizáveis](#componentes-ui-reutilizáveis)
6. [Estados de Interação](#estados-de-interação)
7. [Responsividade](#responsividade)
8. [Acessibilidade (a11y)](#acessibilidade-a11y)

---

## Visão Geral do Design

### Conceito Visual

O SenaPro adota um visual **dark glassmorphism** inspirado na identidade visual da Mega-Sena:

| Elemento | Referência |
|----------|-----------|
| **Paleta** | Fundo escuro profundo (`#0a0f1d`) com acentos em verde Mega-Sena (`#00e676`), azul Caixa (`#00b0ff`) e dourado acumulado (`#ffd600`) |
| **Estilo** | Glassmorphism — cards translúcidos, bordas sutis, blur no header |
| **Tipografia** | `Outfit` (títulos) + `Inter` (corpo de texto) — Google Fonts |
| **Formas** | Esferas arredondadas (bolas de bingo), badges pill-shaped |
| **Animações** | Subtis: bounce-in nas bolas, pulse-glow em destaques, spin em loaders |

### Princípios de UX

1. **Feedback imediato** — cada ação do usuário gera resposta visual (spinner → resultado)
2. **Progressive disclosure** — informações complexas são entregues em camadas (seleção → preview → resultado)
3. **Consistência visual** — todos os cards, botões e alertas seguem o mesmo sistema de tokens CSS
4. **Clareza funcional** — cada página resolve um problema específico do usuário (importar → auditar → gerar)

---

## Design Tokens

Os tokens são definidos em [styles.css](../sena-pro-frontend/src/styles.css) como variáveis CSS (`:root`). Todos os componentes herdam desses valores.

### Cores Temáticas

| Token | Valor | Uso |
|-------|-------|-----|
| `--bg-primary` | `#0a0f1d` | Fundo principal da aplicação |
| `--bg-secondary` | `#131a30` | Fundo de cards e superfícies elevadas |
| `--bg-tertiary` | `#1b2443` | Background de inputs, selects e áreas secundárias |
| `--bg-glass` | `rgba(19, 26, 48, 0.75)` | Fundo com blur do header (glassmorphism) |
| `--border-glass` | `rgba(255, 255, 255, 0.06)` | Bordas sutis de cards e divisórias |

| Token | Valor | Uso |
|-------|-------|-----|
| `--primary` | `#00e676` | Cor primária — botões CTA, seleções, acentos (Verde Mega-Sena) |
| `--primary-hover` | `#00c853` | Hover do botão primário |
| `--primary-glow` | `rgba(0, 230, 118, 0.25)` | Sombra luminosa de elementos primários |
| `--secondary` | `#00b0ff` | Cor secundária — acentos, links (Azul da Caixa) |
| `--secondary-glow` | `rgba(0, 176, 255, 0.25)` | Sombra luminosa de elementos secundários |
| `--accent` | `#ffd600` | Cor de destaque — valores acumulados, prêmios (Dourado) |
| `--accent-glow` | `rgba(255, 214, 0, 0.25)` | Sombra luminosa de elementos de acento |

| Token | Valor | Uso |
|-------|-------|-----|
| `--danger` | `#ff5252` | Erros e estados negativos |
| `--success` | `#00e676` | Sucesso e confirmações (mesmo valor de `--primary`) |

| Token | Valor | Uso |
|-------|-------|-----|
| `--text-primary` | `#f8fafc` | Texto principal (títulos, labels) |
| `--text-secondary` | `#a0aec0` | Texto secundário (descrições, subtítulos) |
| `--text-muted` | `#718096` | Texto terciário (hints, metadados) |

### Tipografia

| Elemento | Fonte | Peso | Tamanho |
|----------|-------|------|---------|
| Títulos (`h1`–`h4`) | **Outfit** | 600–800 | 2.5rem → 1.3rem (escala responsiva) |
| Corpo de texto | Inter | 400 | 0.95rem – 1.1rem |
| Labels / botões | Inter | 500–600 | 0.85rem – 0.95rem |
| Hint / metadados | Inter | 300–400 | 0.75rem – 0.85rem |

### Espaçamento e Bordas

| Token | Valor | Uso |
|-------|-------|-----|
| `--radius-sm` | `8px` | Inputs, badges pequenos, botões |
| `--radius-md` | `16px` | Cards, áreas de drop zone |
| `--radius-lg` | `24px` | Elementos grandes (quando aplicável) |
| Transição padrão | `all 0.3s cubic-bezier(0.4, 0, 0.2, 1)` | Todas as transições interativas |

### Sombras

| Token | Valor | Uso |
|-------|-------|-----|
| `--shadow-lg` | `0 10px 30px -10px rgba(0, 0, 0, 0.5)` | Cards elevados |
| `--shadow-glow` | `0 0 25px rgba(0, 230, 118, 0.15)` | Efeito luminoso de destaque |

---

## Layout & Estrutura da Aplicação

### Hierarquia de Componentes

```
┌──────────────────────────────────────────────────────┐
│  Header (sticky, glass-blur)                         │
│  ┌──────────────────────────────────────────────────┐ │
│  │ Logo  │  Home  │  Sorteios  │  Gerador  │       │ │
│  └──────────────────────────────────────────────────┘ │
├──────────────────────────────────────────────────────┤
│                                                      │
│  Main Content (max-width: 1200px, centralizado)      │
│  ┌──────────────────────────────────────────────────┐ │
│  │                                                  │ │
│  │  [Hero / Page Header]                            │ │
│  │                                                  │ │
│  │  ┌─────────────┐    ┌───────────────────────┐   │ │
│  │  │   Card 1    │    │      Card 2           │   │ │
│  │  │  (Action     │    │  (Result / Analysis)  │   │ │
│  │  │   Panel)     │    │                       │   │ │
│  │  └─────────────┘    └───────────────────────┘   │ │
│  │                                                  │ │
│  └──────────────────────────────────────────────────┘ │
│                                                      │
├──────────────────────────────────────────────────────┤
│  Footer (centrado, border-top)                       │
└──────────────────────────────────────────────────────┘
```

### Grid System

A aplicação usa **CSS Grid** e **Flexbox** como base:

| Layout | Uso | Breakpoint |
|--------|-----|------------|
| `grid-template-columns: 1fr 1fr` | Dashboard split (Sorteios) | ≥992px |
| `grid-template-columns: 4fr 6fr` | Gerador split (config → resultado) | ≥992px |
| `repeat(auto-fit, minmax(320px, 1fr))` | Home dashboard grid | — |
| `repeat(10, 1fr)` | Grade numérica de seleção | — |

**Breakpoint de colapso:** `768px` (mobile) e `992px` (tablet).

---

## Páginas e Navegação

### Rotas da Aplicação

| Rota | Componente | Descrição |
|------|-----------|-----------|
| `/` | Redireciona para `/home` | Entrada padrão |
| `/home` | `HomeComponent` | Importação de planilha histórica |
| `/sorteios` | `SorteiosComponent` | Conferência e auditoria histórica |
| `/gerador` | `GeradorComponent` | Gerador inteligente de jogos |
| `**` (catch-all) | Redireciona para `/home` | Rota inválida |

### Navegação

A navegação é **horizontal** no header fixo:

```
┌──────────────────────────────────────────────────────┐
│  🎰 Sena<span class="highlight">Pro</span>      │  Home  Sorteios  Gerador   │
└──────────────────────────────────────────────────────┘
```

- **Item ativo:** fundo verde translúcido, borda sutil, texto verde primário
- **Hover:** fundo branco a 3% de opacidade
- **Sticky:** header fixo no topo com `backdrop-filter: blur(12px)` (glassmorphism)

### Detalhamento por Página

#### 1. Home — `HomeComponent` (`/home`)

**Objetivo:** Permitir ao usuário importar a planilha histórica oficial de resultados da Mega-Sena.

```
┌─────────────────────────────────────────────┐
│  Hero: "Análise Inteligente Mega-Sena"      │
│  Subtítulo explicativo                        │
├─────────────────────────────────────────────┤
│  ┌───────────────────────────────────────┐  │
│  │ Card: Importar Planilha Histórica     │  │
│  │                                       │  │
│  │  [Área Drag & Drop]                   │  │
│  │    📥 Arraste ou clique para          │  │
│  │    selecionar .xlsx / .xls            │  │
│  │                                       │  │
│  │  [Arquivo Selecionado] [Importar]     │  │
│  │                                       │  │
│  │  [Feedback: Sucesso/Erro]             │  │
│  └───────────────────────────────────────┘  │
└─────────────────────────────────────────────┘
```

**Fluxo de Interação:**
1. Usuário arrasta ou clica na zona de drop
2. Sistema valida extensão (`.xlsx` / `.xls`) e exibe nome do arquivo + tamanho
3. Botão "Iniciar Importação" é habilitado
4. Loading spinner aparece no botão durante requisição
5. Resultado é exibido: contadores de inseridos/ignorados ou lista de erros

#### 2. Sorteios — `SorteiosComponent` (`/sorteios`)

**Objetivo:** Auditar combinações e identificar repetições históricas. Layout dividido em dois painéis.

```
┌─────────────────────────────────────────────┐
│  Page Header: "Conferência & Auditoria"     │
├──────────────────┬──────────────────────────┤
│  Painel Esquerdo │   Painel Direito         │
│  ┌────────────┐  │  ┌────────────────────┐  │
│  │🔍 Testar   │  │  │ 🔄 Sorteios        │  │
│  │Jogo no     │  │  │ Históricos         │  │
│  │Histórico   │  │  │ Idênticos          │  │
│  │            │  │  │                    │  │
│  │ [Grade]    │  │  │  ┌─ Repetidos ─┐  │  │
│  │ 1..60      │  │  │  │ Par 1       │  │  │
│  │            │  │  │  │ (123) (123) │  │  │
│  │ [Selec. N] │  │  │  └─────────────┘  │  │
│  │ /6         │  │  │                    │  │
│  │ [Limpar][A]│  │  └────────────────────┘  │
│  │            │  │                          │
│  │ [Preview B.]│ │                          │
│  │ O O O --   │  │                          │
│  │            │  │                          │
│  │ [Resultado] │  │                          │
│  │ 🎉 Inédito!│  │                          │
│  └────────────┘  │                          │
└──────────────────┴──────────────────────────┘
```

**Fluxo de Interação (Painel Esquerdo):**
1. Usuário clica em números na grade circular 1–60 (máx. 6)
2. Preview visual das bolas selecionadas aparece abaixo
3. Contador `N/6` muda de vermelho → verde quando completo
4. Botão "Auditar Combinação" é habilitado apenas com 6 dezenas
5. Resultado: alerta verde (inédito) ou vermelho (já sorteado)

**Fluxo de Interação (Painel Direito):**
1. Carregamento automático ao entrar na página
2. Se repetidos encontrados → lista scrollável com pares e bolas
3. Se nenhum repetido → alerta de sucesso

#### 3. Gerador — `GeradorComponent` (`/gerador`)

**Objetivo:** Gerar combinações de números baseadas em filtros estatísticos. Layout split config → resultado.

```
┌─────────────────────────────────────────────┐
│  Page Header: "Gerador de Sugestões"        │
├──────────────────┬──────────────────────────┤
│  Configurações   │   Resultados             │
│  ┌────────────┐  │  ┌────────────────────┐  │
│  │⚙️ Parâmetros│  │  │ 🎯 Jogos           │  │
│  │ da Aposta  │  │  │ Sugeridos    [Cop.]│  │
│  │            │  │  │                    │  │
│  │ Dezenas:   │  │  │ ┌─ Jogo #1       │  │
│  │ [6 Padrão] │  │  │ │ O  O  O  O     │  │
│  │            │  │  │ │ O  O           │  │
│  │ Jogos:     │  │  │ │    [📋]        │  │
│  │ [5      ]  │  │  │ └────────────────┘  │  │
│  │            │  │  │                    │  │
│  │ ☑ Evitar   │  │  │ ┌─ Jogo #2       │  │
│  │ Repetidos  │  │  │ │ O  O  O  O     │  │
│  │            │  │  │ │ O  O           │  │
│  │ [Gerar]    │  │  │ │    [📋]        │  │
│  └────────────┘  │  └────────────────────┘  │
│                  │                          │
│                  │  [Empty State / Loading] │
└──────────────────┴──────────────────────────┘
```

**Parâmetros de Geração:**

| Parâmetro | Tipo | Padrão | Faixa |
|-----------|------|--------|-------|
| Dezenas por Jogo | Select | 6 (Padrão) | 6–15 |
| Quantidade de Jogos | Number input | 5 | 1–50 |
| Evitar Sorteios Repetidos | Checkbox | ✅ marcado | — |

**Estados da Área de Resultados:**

| Estado | Condição | Visual |
|--------|----------|--------|
| Empty | Sem geração anterior | Emoji 🎲 + texto instrutivo |
| Loading | Geração em andamento | Spinner grande + texto "Gerando combinações..." |
| Success | Geração concluída | Lista de jogos com bolas animadas (bounce-in) |

---

## Componentes UI Reutilizáveis

### Cards

Baseados na classe `.card` do `styles.css`:

```css
.card {
  background: var(--bg-secondary);
  border: 1px solid var(--border-glass);
  border-radius: var(--radius-md);    /* 16px */
  padding: 2rem;
  box-shadow: var(--shadow-lg);
}
```

**Variantes:**
- `.action-card` — card de ação (Home)
- `.conferidor-card` / `.repetidos-card` — painéis do dashboard (Sorteios)
- `.config-card` / `.resultado-card` — split do Gerador

### Botões

| Classe | Uso | Estilo |
|--------|-----|--------|
| `.btn.btn-primary` | Ação principal (CTA) | Fundo verde (`--primary`), texto escuro |
| `.btn.btn-secondary` | Ação secundária / cancelamento | Transparente com borda sutil |
| `.btn.btn-block` | Botão full-width (Gerador) | `width: 100%` |
| `.btn.btn-sm` | Botão compacto (Copiar Todos) | Padding e font-size reduzidos |

**Estados:**
- **Hover:** `--primary-hover` com glow (`box-shadow`)
- **Disabled:** `opacity: 0.5`, `cursor: not-allowed`
- **Loading:** Spinner substitui texto (classe `.spinner-btn`)

### Alertas (Feedback)

| Classe | Significado | Cor de Borda |
|--------|------------|-------------|
| `.alert-box.success` | Sucesso / confirmado | Verde (`#10b981`) |
| `.alert-box.warning` | Aviso / informação | Amarelo/Laranja (`#f59e0b`) |
| `.alert-box.danger` | Erro / negativo | Vermelho (`#ef4444`) |

### Badges

| Classe | Uso |
|--------|-----|
| `.badge-success` | Identificador de concurso (verde) |
| `.badge-accumulated` | Valor acumulado — com animação pulse-glow (dourado) |

### Bolas (Balls)

Componente visual recorrente que representa dezenas sorteadas:

```css
.ball {
  width: 2.8rem;           /* ~45px */
  height: 2.8rem;
  border-radius: 50%;
  background: radial-gradient(circle at 30% 30%, #10b981, #047857);
  box-shadow: 0 4px 10px rgba(4, 120, 87, 0.35), inset ...;
}
```

**Variantes:**
- `.ball-placeholder` — slot vazio (borda tracejada)
- `.small-balls .ball` — bolas menores em listas de repetidos (`2.2rem`)
- `.anim-ball` — com animação bounce-in sequencial (delay por índice)

### Grade Numérica (1–60)

Usada exclusivamente na página Sorteios:

```css
.number-grid {
  grid-template-columns: repeat(10, 1fr);
  gap: 0.5rem;
}
.grid-number {
  aspect-ratio: 1;          /* quadrado → círculo com border-radius: 50% */
  border-radius: 50%;
  transition: var(--transition);
}
```

**Estados:**
- **Normal:** fundo `--bg-tertiary`, texto `--text-secondary`
- **Hover (não disabled):** fundo mais claro, borda branca a 30%
- **Selecionado:** gradiente radial verde, glow (`box-shadow`)
- **Disabled (limite atingido):** `opacity: 0.25`, cursor not-allowed

### Drag & Drop Zone

Usada exclusivamente na página Home para importação de Excel:

```css
.drag-drop-zone {
  border: 2px dashed rgba(255, 255, 255, 0.15);
  border-radius: var(--radius-md);
  padding: 2.5rem;
  cursor: pointer;
}
.drag-over {
  border-color: var(--primary);
  background: rgba(0, 230, 118, 0.03);
}
```

### Spinner (Loading)

Componente universal de carregamento:

```css
.spinner {
  width: 1.5rem;              /* padrão */
  height: 1.5rem;
  border: 2px solid rgba(255, 255, 255, 0.1);
  border-radius: 50%;
  border-top-color: currentColor;
  animation: spin 0.8s linear infinite;
}
```

**Variantes de tamanho:**
- `.spinner-btn` — dentro de botões (menor)
- `width/height: 3rem–3.5rem` — loading states centrais

---

## Estados de Interação

### Fluxo de Feedback por Tipo de Ação

| Tipo de Ação | Estado Intermediário | Estado Final |
|-------------|---------------------|-------------|
| Importar Excel | Spinner no botão + `loadingImport = true` | Alerta success/danger com contadores ou erros |
| Verificar Jogo | Spinner + estado "verificando" ativado | Alerta vermelho (já sorteado) ou verde (inédito) |
| Gerar Jogos | Spinner grande centralizado + texto descritivo | Lista de jogos com bolas animadas |
| Carregar Repetidos | Spinner centralizado + "Analisando..." | Alerta success (nenhum) ou lista scrollável |

### Transições e Animações

| Animação | Uso | Duração | Easing |
|----------|-----|---------|--------|
| `spin` | Spinners de carregamento | 0.8s linear | linear |
| `bounce-in` | Bolas ao aparecerem na lista | 0.5s | cubic-bezier(0.175, 0.885, 0.32, 1.275) |
| `float` | Emoji no empty state | 3s ease-in-out (loop) | ease-in-out |
| `pulse-glow` | Badges de acumulado | 2s alternate (loop) | alternate |
| `slide-up` | Resultado de verificação aparecendo | 0.3s | cubic-bezier(0.4, 0, 0.2, 1) |

**Transição padrão:** `all 0.3s cubic-bezier(0.4, 0, 0.2, 1)` — aplicada via variável `--transition`.

---

## Responsividade

### Breakpoints

| Breakpoint | Largura | Comportamento |
|-----------|---------|---------------|
| **Desktop** | ≥992px | Layout multi-coluna (grid 2 colunas ou split 4fr/6fr) |
| **Tablet** | 768px–991px | Colapso para coluna única; nav-links com wrap |
| **Mobile** | <768px | Header empilhado verticalmente; padding reduzido (2rem → 1rem) |
| **Small Mobile** | <480px | Grid de ações em coluna simples na Home |

### Adaptações por Página

**Home:** Layout já é single-column — adapta apenas padding e tamanho do hero.

**Sorteios:** `dashboard-split` muda de `grid-template-columns: 1fr 1fr` para `1fr` abaixo de 992px. Grade numérica mantém `repeat(10, 1fr)` mas botões ficam menores em telas estreitas.

**Gerador:** `gerador-split` muda de `4fr 6fr` para `1fr` (config acima, resultados abaixo) em tablets e mobile.

---

## Acessibilidade (a11y)

### Pontos Identificados no Código Atual

| Aspecto | Status | Detalhe |
|---------|--------|---------|
| **Labels semânticos** | ✅ Parcial | Inputs possuem `id` + `<label for>`. Botões de grade usam texto visível (número). |
| **aria-labels** | ⚠️ Pendente | Áreas de drag-drop e botões icônicos (copiar) não possuem `aria-label`. |
| **Foco visível** | ✅ Presente | Outline padrão do navegador; `.form-control:focus` tem glow verde. |
| **Contraste** | ⚠️ Parcial | Texto `--text-muted` (`#718096`) sobre `--bg-primary` (`#0a0f1d`) pode não atingir WCAG AA (4.5:1). |
| **Teclado** | ✅ Funcional | Grade numérica é clicável via tab; selects e inputs são nativos. |
| **Animações** | ⚠️ Parcial | Animações contínuas (`pulse-glow`, `float`) não respeitam `prefers-reduced-motion`. |

### Recomendações de Melhoria

1. Adicionar `aria-label` em botões icônicos (copiar, upload)
2. Respeitar `prefers-reduced-motion` para pausar animações contínuas
3. Verificar contraste do texto `--text-muted` contra fundo (`#718096` sobre `#0a0f1d`) — pode precisar de ajuste para WCAG AA
4. Adicionar `role="status"` e `aria-live` nos alertas de feedback dinâmico

---

## Relação com Outros Documentos de Tech

| Documento | Foco | Quando consultar |
|-----------|------|------------------|
| **[architecture.md](architecture.md)** | Arquitetura do sistema (camadas, padrões) | Quer entender **como** o frontend se conecta ao backend |
| **[development-practices.md](development-practices.md)** | Práticas de desenvolvimento (TDD, testes) | Quer entender **como** o código é escrito e testado |
| **[tech.md](tech.md)** | Stack tecnológico (tecnologias, versões) | Quer saber **quais** tecnologias são usadas no frontend |

---

## Histórico de Alterações

| Data | Versão | Descrição |
|------|--------|-----------|
| 2026-08-05 | v1.0 | Criação do documento com análise completa da UI/UX do SenaPro |
