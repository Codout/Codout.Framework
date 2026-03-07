# Gold Reference — ReceivableAccount

- **Módulo**: Finance
- **Tipo**: Padrão C — CRUD com KPIs e ações operacionais
- **Controller**: `ReceivableAccountController`
- **Base estrutural**: `CrudController<IReceivableAccountRepository, ReceivableAccount, ReceivableAccountViewModel>`

## Views centrais
- `_DataTable`
- `_EditOrCreate`
- `_PayModal`
- `_RescheduleModal`
- `_ApplyDiscountModal`
- `_ApplyFineInterestModal`

## Por que é referência
- Combina CRUD padrão com camada operacional rica.
- Mostra como KPIs convivem com listagem administrativa.
- Mostra uso de modais auxiliares por intenção de negócio.
- Mostra formulário complexo com tabs sem abandonar o núcleo padrão.
