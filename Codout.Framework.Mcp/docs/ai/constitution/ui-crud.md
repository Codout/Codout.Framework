# Constituição UI/CRUD Codout v1

## Objetivo
Definir o contrato obrigatório para construção de telas administrativas e fluxos CRUD no ecossistema Codout sobre Metronic/Keenthemes, usando Codout.Web como base estrutural e Codout.Club como referência de uso real.

O objetivo não é descrever possibilidades do Metronic. O objetivo é restringir a geração de interface para que qualquer tela nova siga a gramática do ecossistema Codout.

## Princípios inegociáveis

### A linguagem do sistema é Codout sobre Metronic
Metronic é infraestrutura visual. Ele não dita a arquitetura da tela.

Toda tela nova deve seguir primeiro:
1. as abstrações do Codout.Web;
2. as convenções do projeto consumidor;
3. só então os recursos visuais do Metronic.

### Não gerar HTML solto quando existir contrato interno
Se existe `CrudController`, `codout-grid`, partial padrão, layout padrão ou helper interno, isso deve ser usado antes de qualquer composição manual baseada em HTML do template.

### Toda tela deve nascer de um padrão conhecido
Antes de criar uma tela, ela deve ser classificada em um tipo canônico:
- CRUD padrão
- CRUD com filtros/KPIs
- formulário complexo
- detalhes com ações auxiliares
- portal/autosserviço
- autenticação
- dashboard

### Repetição controlada é preferível a criatividade estrutural
Padronização vale mais que originalidade local.

### A IA não projeta arquitetura de tela do zero
A IA instancia contratos existentes. Quando houver lacuna, deve optar pela solução mais conservadora e alinhada ao padrão dominante.

## Hierarquia de decisão
1. Convenções do projeto consumidor
2. Contratos do Codout.Web
3. Convenções do Codout.Framework
4. Metronic/Keenthemes
5. Convenções genéricas de ASP.NET MVC/Razor

## Estrutura canônica de aplicação

### Organização modular
A aplicação é organizada por domínio/módulo, com `Areas/<Area>/Views/<Entidade>/...` na camada web.

### Feature administrativa
Uma feature administrativa típica é composta por:
- Controller herdando de `CrudController<...>` quando o caso é CRUD padrão
- ViewModel derivada de base apropriada
- views `_DataTable` e `_EditOrCreate` como núcleo
- partials auxiliares quando houver fluxo especializado
- autorização explícita
- integração com menu/rota padrão

### Exceções permitidas
É aceitável sair do CRUD padrão apenas quando:
- houver fluxo transacional específico
- houver forte necessidade de UX distinta
- houver tela de portal/autosserviço
- houver autenticação/onboarding
- houver dashboard analítico

Mesmo nesses casos, layout, toolbar, cards e convenções de composição devem permanecer consistentes.

## Contrato de layout
- Toda página interna usa layout compartilhado.
- Não recriar header, sidebar, footer ou contêiner principal por feature.
- Sections como toolbar devem seguir o layout oficial.
- Menu vem de provider, não da própria view.

## Contrato de CRUD
- CRUD administrativo clássico usa `CrudController<TRepository, TEntity, TViewModel>`.
- CRUD padrão usa `_DataTable` para listagem e `_EditOrCreate` para criação/edição.
- Views auxiliares podem existir por intenção de negócio, como `_Details`, `_PayModal`, `_RescheduleModal`, `_Cancel`.
- Modal é para ação rápida e contextual; página completa é para formulários e fluxos mais densos.

## Contrato de grids/listagens
- A listagem administrativa padrão usa `codout-grid`.
- Não implementar DataTables manualmente quando o `codout-grid` resolve.
- A toolbar da listagem segue a convenção do grid e do layout.
- Colunas devem usar os tag helpers existentes.
- Filtros devem seguir estrutura visual previsível.

## Contrato de formulários
- Formulários CRUD padrão nascem em `_EditOrCreate`.
- Lookups e dados auxiliares via `BindAsync` ou mecanismo equivalente.
- Organizar campos por grupos semânticos.
- Tabs só quando houver agrupamentos fortes e estáveis.

## Contrato de autorização
- Toda feature administrativa deve declarar autorização usando os atributos e helpers oficiais.
- Visibilidade de ações deve respeitar os mecanismos de verificação já existentes.
- Não espalhar regra de autorização ad hoc em HTML/JS.

## Contrato de ViewModels
- ViewModel é contrato de tela, não cópia cega da entidade.
- Conversões entre domínio e tela seguem o pipeline padrão do projeto.
- Lookups e coleções auxiliares devem ser previsíveis.

## Contrato visual
- Cards são o contêiner visual padrão.
- KPI existe para leitura operacional; não é decoração.
- Toolbar precisa ter hierarquia clara.
- Responsividade deve seguir o grid e os utilitários do tema.

## Anti-padrões proibidos
1. Criar tela administrativa fora do layout padrão.
2. Recriar header, sidebar ou footer localmente.
3. Ignorar `CrudController` em CRUD clássico sem justificativa.
4. Ignorar `_DataTable` ou `_EditOrCreate` sem justificativa.
5. Implementar DataTable manual quando `codout-grid` resolve.
6. Espalhar HTML Metronic cru onde já existe abstração Codout.
7. Montar menu diretamente na view.
8. Espalhar autorização ad hoc em JS/HTML.
9. Criar formulários extensos sem agrupamento semântico.
10. Criar nomenclatura arbitrária de views, partials e controllers.

## Fluxo de decisão para novas telas
1. Classificar o tipo de tela.
2. Localizar a referência ouro mais próxima.
3. Definir controller.
4. Definir ViewModel.
5. Montar views a partir do padrão.
6. Aplicar autorização.
7. Validar aderência ao layout e aos componentes internos.
