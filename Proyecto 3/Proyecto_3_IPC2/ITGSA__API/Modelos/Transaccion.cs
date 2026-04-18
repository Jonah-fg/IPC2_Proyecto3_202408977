namespace ITGSA__API.Modelos
{
    public class Transaccion
    {
        public DateTime Fecha { get; set; }
        public decimal Cargo { get; set; }
        public decimal Abono { get; set; }
        public string Descripcion { get; set; }

        public Transaccion()
        {
            Fecha=DateTime.MinValue;
            Cargo=0;
            Abono=0;
            Descripcion=string.Empty;
        }
    }
}
