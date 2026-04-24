namespace ITGSA__API.Modelos
{
    public class Pago
    {
        public string NitCliente { get; set; }
        public string CodigoBanco { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }
        public string Referencia { get; set; }

        public Pago()
        {
            NitCliente= string.Empty;
            CodigoBanco = string.Empty;
            Fecha =DateTime.MinValue;
            Monto =0;
            Referencia=string.Empty;
        }
    }
}
