
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.Entity;
using System.Globalization;
using AutoMapper;
using SistemaVenta.Entity.Models;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;       
using SistemaVenta.Entity.Models;


namespace SistemaVenta.AplicacionWeb.Utilidades.Automapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            #region Rol
            CreateMap<Rol, VMRol>().ReverseMap();
            #endregion Rol

            #region Usuario
            CreateMap<Usuario, VMUsuario>()
            .ForMember(destino =>
            destino.EsActivo,
            opt => opt.MapFrom(origen => origen.EsActivo == true ? 1 : 0)
            )
            .ForMember(destino =>
            destino.NombreRol,
            opt => opt.MapFrom(origen => origen.IdRolNavigation.Descripcion)
            );

            CreateMap<VMUsuario, Usuario>()
                .ForMember(destino =>
                destino.EsActivo,
                opt => opt.MapFrom(origen => origen.EsActivo == 1 ? true : false)
                )
                .ForMember(destino =>
                destino.IdRolNavigation,
                opt => opt.Ignore()
                );

            #endregion


            #region Negocio
            CreateMap<Negocio, VMNegocio>()
                .ForMember(destino =>
                destino.PorcentajeImpuesto,
                opt => opt.MapFrom(origen => Convert.ToString(origen.PorcentajeImpuesto.Value, new CultureInfo("Es-MEX")))
                );

            CreateMap<VMNegocio, Negocio>()
                .ForMember(destino =>
                destino.PorcentajeImpuesto,
                opt => opt.MapFrom(origen => Convert.ToDecimal(origen.PorcentajeImpuesto, new CultureInfo("Es-MEX")))
                );

            #endregion


            #region Categoria

            CreateMap<Categoria, VMCategoria>()

                .ForMember(destino =>

                destino.EsActivo,

                opt => opt.MapFrom(origen => origen.EsActivo == true ? 1 : 0));

            CreateMap<VMCategoria, Categoria>()

                .ForMember(destino =>

                destino.EsActivo,

                opt => opt.MapFrom(origen => origen.EsActivo == 1 ? true : false));

            #endregion


            #region Producto
            CreateMap<Producto, VMProducto>()
                .ForMember(destino =>
                destino.EsActivo,
                opt => opt.MapFrom(origen => origen.EsActivo == true ? 1 : 0)
                )
                .ForMember(destino =>
                destino.NombreCategoria,
                opt => opt.MapFrom(origen => origen.IdCategoriaNavigation.Descripcion)
                )
                .ForMember(destino =>
                destino.Precio,
                opt => opt.MapFrom(origen => Convert.ToString(origen.Precio.Value, new CultureInfo("Es-MEX")))
                );

            CreateMap<VMProducto, Producto>()
                .ForMember(destino =>
                destino.EsActivo,
                opt => opt.MapFrom(origen => origen.EsActivo == 1 ? true : false)
                )
                .ForMember(destino =>
                destino.Precio,
                opt => opt.MapFrom(origen => Convert.ToDecimal(origen.Precio, new CultureInfo("Es-MEX")))
                );
            #endregion


            #region TipoDocumentoVenta
            CreateMap<TipoDocumentoVenta, VMTipoDocumentoVenta>().ReverseMap();
            #endregion

            #region Venta
            CreateMap<Venta, VMVenta>()
                .ForMember(destino =>
                destino.TipoDocumentoVenta,
                opt => opt.MapFrom(origen => origen.IdTipoDocumentoVentaNavigation.Descripcion)
                )
                .ForMember(destino =>
                destino.Usuario,
                opt => opt.MapFrom(origen => origen.IdUsuarioNavigation.Nombre)
                )
                .ForMember(destino =>
                destino.SubTotal,
                opt => opt.MapFrom(origen => Convert.ToString(origen.SubTotal.Value, new CultureInfo("Es-MEX")))
                )
                .ForMember(destino =>
                destino.ImpuestoTotal,
                opt => opt.MapFrom(origen => Convert.ToString(origen.ImpuestoTotal.Value, new CultureInfo("Es-MEX")))
                )
                .ForMember(destino =>
                destino.Total,
                opt => opt.MapFrom(origen => Convert.ToString(origen.Total.Value, new CultureInfo("Es-MEX")))
                )
                .ForMember(destino =>
                destino.FechaRegistro,
                opt => opt.MapFrom(origen => origen.FechaRegistro.Value.ToString("dd/MM/yyyy"))
                );

            CreateMap<VMVenta, Venta>()
                .ForMember(destino =>
                destino.SubTotal,
                opt => opt.MapFrom(origen => Convert.ToDecimal(origen.SubTotal, new CultureInfo("Es-MEX")))
                )
                .ForMember(destino =>
                destino.ImpuestoTotal,
                opt => opt.MapFrom(origen => Convert.ToDecimal(origen.ImpuestoTotal, new CultureInfo("Es-MEX")))
                )
                .ForMember(destino =>
                destino.Total,
                opt => opt.MapFrom(origen => Convert.ToDecimal(origen.Total, new CultureInfo("Es-MEX")))
                );
            #endregion

            #region DetalleVenta
            CreateMap<DetalleVenta, VMDetalleVenta>()
                .ForMember(destino =>
                destino.Precio,
                opt => opt.MapFrom(origen => Convert.ToString(origen.Precio.Value, new CultureInfo("Es-MEX")))
                )
                .ForMember(destino =>
                destino.Total,
                opt => opt.MapFrom(origen => Convert.ToString(origen.Total.Value, new CultureInfo("Es-MEX")))
                );
            CreateMap<VMDetalleVenta, DetalleVenta>()
                .ForMember(destino =>
                destino.Precio,
                opt => opt.MapFrom(origen => Convert.ToDecimal(origen.Precio, new CultureInfo("Es-MEX")))
                )
                .ForMember(destino =>
                destino.Total,
                opt => opt.MapFrom(origen => Convert.ToDecimal(origen.Total, new CultureInfo("Es-MEX")))
                );

            CreateMap<DetalleVenta, VMReporteVenta>()
    .ForMember(destino => destino.FechaRegistro,
        opt => opt.MapFrom(origen => origen.IdVentaNavigation.FechaRegistro.Value.ToString("dd/MM/yyyy")))
    .ForMember(destino => destino.NumeroVenta,
        opt => opt.MapFrom(origen => origen.IdVentaNavigation.NumeroVenta))
    .ForMember(destino => destino.TipoDocumento,
        opt => opt.MapFrom(origen => origen.IdVentaNavigation.IdTipoDocumentoVentaNavigation.Descripcion))
    .ForMember(destino => destino.DocumentoCliente,
        opt => opt.MapFrom(origen => origen.IdVentaNavigation.DocumentoCliente))
    .ForMember(destino => destino.NombreCliente,
        opt => opt.MapFrom(origen => origen.IdVentaNavigation.NombreCliente))
    .ForMember(destino => destino.SubTotalVenta,
        opt => opt.MapFrom(origen => Convert.ToString(origen.IdVentaNavigation.SubTotal.Value, new CultureInfo("es-MEX"))))
    // --- CORRECCIÓN 1: Agregamos el Impuesto que faltaba ---
    .ForMember(destino => destino.ImpuestoTotalVenta,
        opt => opt.MapFrom(origen => Convert.ToString(origen.IdVentaNavigation.ImpuestoTotal.Value, new CultureInfo("es-MEX"))))

    // --- CORRECCIÓN 2: El TotalVenta ahora apunta al TOTAL real, no al impuesto ---
    .ForMember(destino => destino.TotalVenta,
        opt => opt.MapFrom(origen => Convert.ToString(origen.IdVentaNavigation.Total.Value, new CultureInfo("es-MEX"))))

    .ForMember(destino => destino.Producto,
        opt => opt.MapFrom(origen => origen.DescripcionProducto))
    .ForMember(destino => destino.Precio,
        opt => opt.MapFrom(origen => Convert.ToString(origen.Precio.Value, new CultureInfo("es-MEX"))))
    .ForMember(destino => destino.Total,
        opt => opt.MapFrom(origen => Convert.ToString(origen.Total.Value, new CultureInfo("es-MEX"))));
            #endregion

            #region Menu
            CreateMap<Menu, VMMenu>()
                .ForMember(destino =>
                destino.SubMenus,
                opt => opt.MapFrom(origen => origen.InverseIdMenuPadreNavigation)
                );
            #endregion



        }
    }
}