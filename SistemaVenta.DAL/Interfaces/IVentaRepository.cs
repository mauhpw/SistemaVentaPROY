using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using SistemaVenta.Entity;
using SistemaVenta.Entity.Models;

namespace SistemaVenta.DAL.Interfaces
{
    public interface IVentaRepository : IGenericRepository<Venta>
    {
        Task<Venta> Registrar(Venta entidad);

        Task<List<DetalleVenta>> Reporte(DateTime fechaInicio, DateTime fechaFin);

    }
}
