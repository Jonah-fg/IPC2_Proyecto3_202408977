using ITGSA__API.Modelos;
using System.Xml.Serialization;

namespace ITGSA.API.Services
{
    public class AlmacenamientoXml
    {
        private readonly string _rutaDatos;

        public AlmacenamientoXml()
        {
            _rutaDatos =Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Datos");
            if (!Directory.Exists(_rutaDatos))
                Directory.CreateDirectory(_rutaDatos);
        }

        // clientes
        public void GuardarClientes(List<Cliente> clientes)
        {
            string archivo= Path.Combine(_rutaDatos, "clientes.xml");
            XmlSerializer serializador= new XmlSerializer(typeof(List<Cliente>));
            using (StreamWriter escritor =new StreamWriter(archivo))
            {
                serializador.Serialize(escritor, clientes);
            }
        }

        public List<Cliente> CargarClientes()
        {
            string archivo=Path.Combine(_rutaDatos, "clientes.xml");
            if (!File.Exists(archivo)) 
                return new List<Cliente>();

            XmlSerializer serializador= new XmlSerializer(typeof(List<Cliente>));
            using (StreamReader lector= new StreamReader(archivo))
            {
                return (List<Cliente>)serializador.Deserialize(lector);
            }
        }

        // Bancos
        public void GuardarBancos(List<Banco> bancos)
        {
            string archivo =Path.Combine(_rutaDatos, "bancos.xml");
            XmlSerializer serializador = new XmlSerializer(typeof(List<Banco>));
            using (StreamWriter escritor = new StreamWriter(archivo))
            {
                serializador.Serialize(escritor, bancos);
            }
        }

        public List<Banco> CargarBancos()
        {
            string archivo =Path.Combine(_rutaDatos, "bancos.xml");
            if (!File.Exists(archivo)) 
                return new List<Banco>();

            XmlSerializer serializador= new XmlSerializer(typeof(List<Banco>));
            using (StreamReader lector=new StreamReader(archivo))
            {
                return (List<Banco>)serializador.Deserialize(lector);
            }
        }

        // facturas
        public void GuardarFacturas(List<Factura> facturas)
        {
            string archivo =Path.Combine(_rutaDatos, "facturas.xml");
            XmlSerializer serializador= new XmlSerializer(typeof(List<Factura>));
            using (StreamWriter escritor=new StreamWriter(archivo))
            {
                serializador.Serialize(escritor, facturas);
            }
        }

        public List<Factura> CargarFacturas()
        {
            string archivo = Path.Combine(_rutaDatos, "facturas.xml");
            if (!File.Exists(archivo)) return new List<Factura>();
            XmlSerializer serializador = new XmlSerializer(typeof(List<Factura>));
            using (StreamReader lector = new StreamReader(archivo))
            {
                return (List<Factura>)serializador.Deserialize(lector);
            }
        }

        //pagos
        public void GuardarPagos(List<Pago> pagos)
        {
            string archivo =Path.Combine(_rutaDatos, "pagos.xml");
            XmlSerializer serializador =new XmlSerializer(typeof(List<Pago>));
            using (StreamWriter escritor= new StreamWriter(archivo))
            {
                serializador.Serialize(escritor, pagos);
            }
        }

        public List<Pago> CargarPagos()
        {
            string archivo=Path.Combine(_rutaDatos, "pagos.xml");
            if (!File.Exists(archivo)) 
                return new List<Pago>();

            XmlSerializer serializador =new XmlSerializer(typeof(List<Pago>));
            using (StreamReader lector =new StreamReader(archivo))
            {
                return (List<Pago>)serializador.Deserialize(lector);
            }
        }

        public void ResetearDatos()
        {
            string[] archivos={ "clientes.xml", "bancos.xml", "facturas.xml", "pagos.xml" };
            foreach (string archivo in archivos)
            {
                string rutaCompleta= Path.Combine(_rutaDatos, archivo);
                if (File.Exists(rutaCompleta)) File.Delete(rutaCompleta);
            }
        }
    }
}