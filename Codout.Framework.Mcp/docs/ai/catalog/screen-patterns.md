# Catálogo de padrões de tela

## Padrão A — CRUD administrativo simples
**Quando usar**:
- cadastro clássico
- poucos filtros
- sem ações transacionais complexas

**Composição**:
- `CrudController`
- `_DataTable`
- `_EditOrCreate`
- `codout-grid`
- autorização explícita

## Padrão B — CRUD administrativo com filtros avançados
**Quando usar**:
- maior volume de dados
- combinação de filtros operacionais

**Composição**:
- base do CRUD simples
- card/área de filtros consistente
- ações de busca/reset previsíveis

## Padrão C — CRUD com KPIs e ações operacionais
**Quando usar**:
- mistura visão gerencial e operação
- KPIs ajudam leitura do estado
- há ações auxiliares importantes

**Composição**:
- toolbar clara
- cards KPI objetivos
- filtros quando necessários
- grid principal
- modais auxiliares bem delimitados

## Padrão D — Formulário complexo com subpartes
**Quando usar**:
- muitos campos
- subdomínios claros
- partials internas ou tabs

## Padrão E — Detalhes com ações auxiliares
**Quando usar**:
- leitura consolidada da entidade
- ações como pagar, cancelar, reagendar, renovar, aprovar

## Padrão F — Portal / autosserviço
**Quando usar**:
- tela orientada ao usuário final
- fluxo fora do administrativo tradicional

## Padrão G — Auth / onboarding
**Quando usar**:
- login
- recuperação de senha
- cadastro inicial
- ativação

## Padrão H — Dashboard operacional
**Quando usar**:
- leitura consolidada
- atalho operacional
- visão gerencial compacta
