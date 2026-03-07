# CLAUDE.md — Regras operacionais para geração de telas no ecossistema Codout

## Missão
Gerar código aderente ao padrão do ecossistema Codout sobre Metronic.

Você não deve inventar uma arquitetura de tela nova. Deve identificar e instanciar o padrão dominante já existente no projeto.

## Hierarquia de decisão
1. Convenções do projeto consumidor
2. Contratos do Codout.Web
3. Convenções do Codout.Framework
4. Metronic/Keenthemes
5. Convenções genéricas de ASP.NET MVC/Razor

Se houver conflito, a prioridade mais alta vence.

## Regras principais
1. Se a feature for um CRUD administrativo clássico, use `CrudController<TRepository, TEntity, TViewModel>`.
2. Para CRUD padrão, use `_DataTable` para listagem e `_EditOrCreate` para formulário.
3. Para listagens administrativas, use `codout-grid` e helpers relacionados sempre que possível.
4. Não recrie header, sidebar, footer, toolbar global ou layout por feature.
5. Não implemente DataTables manualmente quando o `codout-grid` resolver.
6. Não use HTML Metronic cru quando já existir abstração interna equivalente.
7. Use `BindAsync` ou mecanismo equivalente para lookups e dados auxiliares.
8. Use atributos e helpers oficiais para autorização e visibilidade, como `UserAuthorize`.
9. Partials devem ser nomeadas por intenção de negócio, não por detalhe visual arbitrário.
10. Formulários grandes devem ser agrupados por seções ou partials; tabs só quando realmente fizerem sentido.

## Tipos de tela canônicos
- CRUD simples
- CRUD com filtros avançados
- CRUD com KPIs e ações operacionais
- formulário complexo
- detalhes com ações auxiliares
- portal/autosserviço
- autenticação
- dashboard

## Fluxo obrigatório antes de codar
1. Identificar o tipo da tela.
2. Procurar a referência ouro mais próxima.
3. Verificar se a feature cabe em `CrudController`.
4. Verificar se a listagem cabe em `_DataTable` + `codout-grid`.
5. Verificar se o formulário cabe em `_EditOrCreate`.
6. Aplicar autorização explícita.
7. Validar que nenhuma abstração paralela foi criada desnecessariamente.

## Anti-padrões proibidos
- Criar tela administrativa fora do layout padrão.
- Ignorar `CrudController` em CRUD clássico sem justificativa.
- Ignorar `_DataTable` / `_EditOrCreate` sem justificativa.
- Montar menu diretamente na view.
- Duplicar toolbar ou layout global na feature.
- Espalhar autorização ad hoc em JS/HTML quando houver helper oficial.
- Criar formulários extensos sem agrupamento semântico.
- Criar listagem manual inconsistente com `codout-grid`.
- Introduzir nomenclatura arbitrária de views/partials/controllers.
- Gerar tela bonita que não respeita o padrão estrutural do projeto.

## Saída esperada ao gerar feature
Ao implementar uma feature, entregue de forma coerente:
- controller
- ViewModel
- views principais
- partials auxiliares quando necessário
- integração com grid, toolbar, auth e layout

## Estratégia de exceção
Se a tela não se encaixar no CRUD padrão, escolha o padrão estrutural mais conservador possível e deixe explícito por que a exceção existe.
