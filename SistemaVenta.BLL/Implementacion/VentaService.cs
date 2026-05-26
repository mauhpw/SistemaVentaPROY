using Microsoft.EntityFrameworkCore;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;
using SistemaVenta.Entity.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaVenta.BLL.Implementacion
{
    public class VentaService : IVentaService
    {
        private readonly IGenericRepository<Producto> _repositorioProducto;
        private readonly IVentaRepository _repositorioVenta;
        private readonly IGenericRepository<Venta> _repositorioVentaGenerico;

        public VentaService(IGenericRepository<Producto> repositorioProducto,
            IVentaRepository repositorioVenta,
            IGenericRepository<Venta> repositorioVentaGenerico)
        {
            _repositorioProducto = repositorioProducto;
            _repositorioVenta = repositorioVenta;
            _repositorioVentaGenerico = repositorioVentaGenerico;
        }

        public async Task<List<Producto>> ObtenerProductos(string busqueda)
        {
            IQueryable<Producto> query = await _repositorioProducto.Consultar(p =>
                p.EsActivo == true &&
                p.Stock > 0 &&
                string.Concat(p.CodigoBarra, p.Marca, p.Descripcion).Contains(busqueda));
            return query.Include(c => c.IdCategoriaNavigation).ToList();
        }

        public async Task<Venta> Registrar(Venta entidad)
        {
            try
            {
                return await _repositorioVenta.Registrar(entidad);
            }
            catch { throw; }
        }

        public async Task<List<Venta>> Historial(string numeroVenta, string fechaInicio, string fechaFin)
        {
            IQueryable<Venta> query = await _repositorioVenta.Consultar();
            fechaInicio = fechaInicio is null ? "" : fechaInicio;
            fechaFin = fechaFin is null ? "" : fechaFin;

            if (fechaInicio != "" && fechaFin != "")
            {
                DateTime fech_inicio = DateTime.ParseExact(fechaInicio, "dd/MM/yyyy", new CultureInfo("es-MEX"));
                DateTime fech_fin = DateTime.ParseExact(fechaFin, "dd/MM/yyyy", new CultureInfo("es-MEX"));
                return query.Where(v =>
                    v.FechaRegistro.Value.Date >= fech_inicio.Date &&
                    v.FechaRegistro.Value.Date <= fech_fin.Date)
                    .Include(tdv => tdv.IdTipoDocumentoVentaNavigation)
                    .Include(u => u.IdUsuarioNavigation)
                    .Include(dv => dv.DetalleVenta)
                    .ToList();
            }
            else
            {
                return query.Where(v => v.NumeroVenta == numeroVenta)
                    .Include(tdv => tdv.IdTipoDocumentoVentaNavigation)
                    .Include(u => u.IdUsuarioNavigation)
                    .Include(dv => dv.DetalleVenta)
                    .ToList();
            }
        }

        public async Task<Venta> Detalle(string numeroVenta)
        {
            IQueryable<Venta> query = await _repositorioVenta.Consultar(v => v.NumeroVenta == numeroVenta);
            return query
                .Include(tdv => tdv.IdTipoDocumentoVentaNavigation)
                .Include(u => u.IdUsuarioNavigation)
                .Include(dv => dv.DetalleVenta)
                .First();
        }

        public async Task<List<DetalleVenta>> Reporte(string fechaInicio, string fechaFin)
        {
            DateTime fech_inicio = DateTime.ParseExact(fechaInicio, "dd/MM/yyyy", new CultureInfo("es-MEX"));
            DateTime fech_fin = DateTime.ParseExact(fechaFin, "dd/MM/yyyy", new CultureInfo("es-MEX"));
            return await _repositorioVenta.Reporte(fech_inicio, fech_fin);
        }

        public async Task<Venta> GuardarDatosFactura(string numeroVenta, string usoCFDI, string regimenFiscal, string formaPago, string metodoPago, string tipoComprobante)
        {
            try
            {
                IQueryable<Venta> query = await _repositorioVenta.Consultar(v => v.NumeroVenta == numeroVenta);
                Venta venta = query.First();
                venta.IdUsoCFDI = int.Parse(usoCFDI);
                venta.IdRegimenFiscal = int.Parse(regimenFiscal);
                venta.IdFormaPago = int.Parse(formaPago);
                venta.IdMetodoPago = int.Parse(metodoPago);
                venta.IdTipoDeComprobante = int.Parse(tipoComprobante);
                venta.Uuid = Guid.NewGuid().ToString();
                venta.FechaTimbrado = DateTime.Now;

                await _repositorioVentaGenerico.Editar(venta);
                return venta;
            }
            catch { throw; }
        }
    }
}