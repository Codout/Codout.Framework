# Gold Reference — MembershipContract

- **Módulo**: Membership
- **Tipo**: Padrão E — detalhes com ações auxiliares + formulário complexo
- **Controller**: `MembershipContractController`
- **Base estrutural**: `CrudController<IMembershipContractRepository, MembershipContract, MembershipContractViewModel>`

## Views centrais
- `_DataTable`
- `_EditOrCreate`
- `_Details`
- `_ContractData`
- `_ContractHistory`
- `_DependentsList`
- `_DependentDocumentsList`
- `_DocumentsList`
- `_StatusHistoryList`
- `_AddDependent`
- `_EditDependent`
- `_UploadDocument`

## Por que é referência
- Mostra decomposição séria em partials por subdomínio.
- Mostra uma tela de detalhes rica sem virar bagunça.
- Mostra convivência coerente entre anexos, dependentes, histórico e status.
