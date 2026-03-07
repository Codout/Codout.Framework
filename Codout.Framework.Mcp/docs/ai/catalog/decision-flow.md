# Fluxo de decisão para novas telas

## Perguntas obrigatórias
1. A tela é CRUD padrão ou exceção?
2. Existe referência ouro semelhante?
3. Pode usar `CrudController`?
4. Pode usar `_DataTable` e `_EditOrCreate`?
5. Pode usar `codout-grid`?
6. Há necessidade real de modal?
7. Há KPIs, filtros ou tabs de verdade?
8. Quais regras de autorização governam a tela?

## Sequência de implementação
1. Classificar o tipo de tela.
2. Localizar a referência ouro mais próxima.
3. Definir controller.
4. Definir ViewModel.
5. Montar views a partir do padrão.
6. Aplicar autorização.
7. Validar aderência ao layout e aos componentes internos.
