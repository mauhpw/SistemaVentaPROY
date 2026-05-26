using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaVenta.Entity.Models;

public partial class DetalleVenta
{
    public int IdDetalleVenta { get; set; }

    public int? IdVenta { get; set; }

    [Column("idProducto")]
    public int? IdProducto { get; set; }

    [Column("marcaProducto")]
    public string? MarcaProducto { get; set; }

    [Column("descripcionProducto")]
    public string? DescripcionProducto { get; set; }

    [Column("categoriaProducto")]
    public string? CategoriaProducto { get; set; }

    public int? Cantidad { get; set; }

    public decimal? Precio { get; set; }

    public decimal? Total { get; set; }

    public virtual Venta? IdVentaNavigation { get; set; }
}