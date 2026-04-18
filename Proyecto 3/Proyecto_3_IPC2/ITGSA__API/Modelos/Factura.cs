using static System.Net.Mime.MediaTypeNames;

namespace ITGSA__API.Modelos
{
    public class Factura
    {
        public string NitCliente { get; set; }
        public int NumeroFactura { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }
        public decimal SaldoPendiente { get; set; }
        public bool Pagada { get; set; }

        public Factura()
        {
            NitCliente=string.Empty;
            NumeroFactura=0;
            Fecha =DateTime.MinValue;  
            Monto=0;
            SaldoPendiente=0;
            Pagada=false;
        }
    }
}

