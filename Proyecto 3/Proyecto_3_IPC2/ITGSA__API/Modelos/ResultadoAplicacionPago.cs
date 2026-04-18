namespace ITGSA__API.Modelos
{
    public class ResultadoAplicacionPago
    {
        public decimal SaldoFavor { get; set; }
        public List<Factura> FacturasAfectadas { get; set; }

        public ResultadoAplicacionPago()
        {
            FacturasAfectadas = new List<Factura>();
        }
    }
}
