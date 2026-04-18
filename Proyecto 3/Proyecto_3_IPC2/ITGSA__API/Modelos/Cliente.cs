namespace ITGSA__API.Modelos
{
    public class Cliente
    {
        public string Nit { get; set; } 
        public string Nombre { get; set; } 
        public string Direccion { get; set; } 
        public decimal SaldoFavor { get; set; } 

        public Cliente()
        {
            Nit=string.Empty;
            Nombre= string.Empty;
            Direccion =string.Empty;
            SaldoFavor=0;
        }
    }
}
