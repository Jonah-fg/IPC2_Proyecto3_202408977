using ITGSA__API.Modelos;

namespace ITGSA__API.Servicios
{
    public class AplicadorPagos
    {

        public ResultadoAplicacionPago AplicarPago(string nitCliente, decimal montoPago, List<Factura> facturas)
        {
            ResultadoAplicacionPago resultado = new ResultadoAplicacionPago();

            // Obtener facturas no pagadas del cliente, ordenadas por fecha (más antigua primero)
            List<Factura> facturasPendientes = new List<Factura>();
            foreach (Factura factura in facturas)
            {
                if (factura.NitCliente==nitCliente && !factura.Pagada)
                {
                    facturasPendientes.Add(factura);
                }
            }
            facturasPendientes.Sort((f1, f2) => f1.Fecha.CompareTo(f2.Fecha));

            decimal montoRestante=montoPago;

            foreach (Factura factura in facturasPendientes)
            {
                if (montoRestante <=0)
                    break;

                decimal pendiente=factura.SaldoPendiente;
                if (montoRestante >=pendiente)
                {
                    montoRestante -=pendiente;
                    factura.SaldoPendiente= 0;
                    factura.Pagada= true;
                    resultado.FacturasAfectadas.Add(factura);
                }
                else
                {
                    factura.SaldoPendiente -=montoRestante;
                    montoRestante = 0;
                    resultado.FacturasAfectadas.Add(factura);
                }
            }
            resultado.SaldoFavor=montoRestante;
            return resultado;
        }
    }
}

