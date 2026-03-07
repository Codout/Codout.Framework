```cshtml
@{
    ViewData["Title"] = "Entities";
}

<codout-grid ajax-url="@Url.Action(nameof(EntityController.DataHandler))">
    <codout-grid-toolbar>
        <!-- ações principais -->
    </codout-grid-toolbar>

    <codout-grid-column field="Name" title="Nome" />
    <codout-grid-column field="IsActive" title="Ativo" />
</codout-grid>
```