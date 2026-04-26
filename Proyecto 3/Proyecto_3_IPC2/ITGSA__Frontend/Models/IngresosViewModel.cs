namespace ITGSA__Frontend.Models
{
    public class IngresosViewModel
    {
        public DateTime MesSeleccionado { get; set; }
        public List<MesIngreso>Meses { get; set; }
    }

    public class MesIngreso
    {
        public string Nombre { get; set; }
        public decimal Total { get; set; }
    }
}
