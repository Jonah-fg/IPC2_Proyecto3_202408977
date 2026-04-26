namespace ITGSA__Frontend.Models
{
    public class EstadoCuentaViewModel
    {
        public string Nit { get; set; }
        public string Nombre { get; set; }
        public decimal SaldoActual { get; set; }
        public List<TransaccionViewModel> Transacciones { get; set; }
    }
}
