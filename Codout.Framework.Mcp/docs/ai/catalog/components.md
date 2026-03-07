# Catálogo de componentes e abstrações canônicas

## CrudController<TRepository, TEntity, TViewModel>
**Papel**: contrato padrão para features CRUD administrativas.

**Usar quando**:
- cadastro, listagem, edição e exclusão clássicos
- integração natural com listagem server-side

**Não usar quando**:
- dashboard
- portal
- onboarding
- fluxo transacional altamente específico

**Entregas esperadas**:
- `Index()`
- `Create()` / `Edit()`
- `Delete()`
- `DataHandler()`
- `ToModelAsync()` e `ToDomainAsync()`

## ViewModelBase e bases equivalentes
**Papel**: dar previsibilidade ao contrato de tela.

**Usar para**:
- lookups
- flags de UI
- coleções auxiliares
- estado de renderização

## BindAsync
**Papel**: carregar dados auxiliares de formulário.

**Usar para**:
- combos
- listas de seleção
- dados contextuais do formulário

## codout-grid
**Papel**: listagem administrativa padrão.

**Benefícios**:
- padronização visual
- menos repetição
- integração com toolbar e colunas
- aderência ao pipeline server-side

## CodoutGridColumnTagHelper
**Papel**: declarar colunas de maneira canônica.

## CodoutGridToolbarTagHelper
**Papel**: toolbar padronizada da listagem.

## _DataTable
**Papel**: view padrão de listagem de uma feature CRUD.

## _EditOrCreate
**Papel**: view padrão de criação/edição.

## UserAuthorize e helpers equivalentes
**Papel**: autorização e visibilidade consistentes.

## Layout compartilhado + partials de shell
**Papel**: preservar header, sidebar, footer, toolbar e containers padronizados.
