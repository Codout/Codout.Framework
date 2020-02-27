using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Codout.Multitenancy
{
    public enum DataBaseType
    {
        [Display(Name = "Postgres")]
        [Description("Postgres")]
        Postgres,

        [Display(Name = "Mssql")]
        [Description("Mssql")]
        MsSql,

        [Display(Name = "Oracle")]
        [Description("Oracle")]
        Oracle

}
}