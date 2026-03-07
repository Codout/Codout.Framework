# Gold Reference — Person

- **Módulo**: Core
- **Tipo**: Padrão D — formulário complexo com subpartes reutilizáveis
- **Controller**: `PersonController`
- **Base estrutural**: `CrudController<IPersonRepository, Person, PersonCrudViewModel>`

## Views centrais
- `_DataTable`
- `_EditOrCreate`
- `_QuickCreatePerson`
- `_PersonForm`
- `_PersonSelect`

## Por que é referência
- Mostra o formulário reutilizável mais importante do projeto.
- Prova que partial reutilizável pode ser parametrizada sem perder legibilidade.
- Serve de base para múltiplos fluxos com seleção ou cadastro de pessoa.
