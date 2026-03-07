# CRUD Recipe

1. Criar ViewModel da entidade.
2. Criar controller herdando de `CrudController<IEntityRepository, Entity, EntityViewModel>`.
3. Criar `_DataTable`.
4. Criar `_EditOrCreate`.
5. Configurar `codout-grid`.
6. Aplicar `UserAuthorize`.
7. Implementar `BindAsync`, `ToModelAsync` e `ToDomainAsync` conforme necessário.
