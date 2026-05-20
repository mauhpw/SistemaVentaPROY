using System;
using System.Collections.Generic;

namespace SistemaVenta.Entity.Models;

public partial class DetalleVenta
{
    public int IdDetalleVenta { get; set; }

    public int? IdVenta { get; set; }

    public int? IdProducto { get; set; }

    public string? MarcaProdutcto { get; set; }

    public string? DescripcionProducto { get; set; }

    public string? CategoriaProducto { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public int? Cantidad { get; set; }

    public decimal? Precio { get; set; }

    public decimal? Total { get; set; }

    public virtual Venta? IdVentaNavigation { get; set; }
}
