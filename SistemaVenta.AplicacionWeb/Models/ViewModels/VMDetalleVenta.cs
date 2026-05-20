using SistemaVenta.Entity.Models;
using System.Text.Json.Serialization;

namespace SistemaVenta.AplicacionWeb.Models.ViewModels
{
    public class VMDetalleVenta
    {

        public int? IdProducto { get; set; }

        public string? MarcaProdutcto { get; set; }

        public string? DescripcionProducto { get; set; }

        public string? CategoriaProducto { get; set; }

        public int? Cantidad { get; set; }

        public string? Precio { get; set; }

        public string? Total { get; set; }
        [JsonIgnore]
        public VMVenta IdVentaNavigation { get; internal set; }
    }
}
