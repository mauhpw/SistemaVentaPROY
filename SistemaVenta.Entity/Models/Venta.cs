using System;
using System.Collections.Generic;

namespace SistemaVenta.Entity.Models;

public partial class Venta
{
    public int IdVenta { get; set; }
    public string? NumeroVenta { get; set; }
    public int? IdTipoDocumentoVenta { get; set; }
    public int? IdUsuario { get; set; }
    public string? DocumentoCliente { get; set; }
    public string? NombreCliente { get; set; }
    public decimal? SubTotal { get; set; }
    public decimal? ImpuestoTotal { get; set; }
    public decimal? Total { get; set; }
    public DateTime? FechaRegistro { get; set; }
    public string? Uuid { get; set; }
    public int? IdUsoCFDI { get; set; }
    public int? IdRegimenFiscal { get; set; }
    public int? IdFormaPago { get; set; }
    public int? IdMetodoPago { get; set; }
    public int? IdTipoDeComprobante { get; set; }
    public DateTime? FechaTimbrado { get; set; }
    public virtual ICollection<DetalleVenta> DetalleVenta { get; set; } = new List<DetalleVenta>();
    public virtual TipoDocumentoVenta? IdTipoDocumentoVentaNavigation { get; set; }
    public virtual Usuario? IdUsuarioNavigation { get; set; }
}