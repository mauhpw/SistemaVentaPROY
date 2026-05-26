using AutoMapper;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Mvc;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.AplicacionWeb.Utilidades.Response;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.Entity.Models;
using System.Security.Claims;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    //[Authorize]
    public class VentaController : Controller
    {
        private readonly ITipoDocumentoVentaService _tipoDocumentoVentaServicio;
        private readonly IVentaService _ventaServicio;
        private readonly IMapper _mapper;
        private readonly IConverter _converter;

        public VentaController(ITipoDocumentoVentaService tipoDocumentoVentaServicio,
            IVentaService ventaServicio,
            IMapper mapper,
            IConverter converter)
        {
            _tipoDocumentoVentaServicio = tipoDocumentoVentaServicio;
            _ventaServicio = ventaServicio;
            _mapper = mapper;
            _converter = converter;
        }

        public IActionResult NuevaVenta() => View();
        public IActionResult HistorialVenta() => View();

        [HttpGet]
        public async Task<IActionResult> ListaTipoDocumentoVenta()
        {
            List<VMTipoDocumentoVenta> vmLista = _mapper.Map<List<VMTipoDocumentoVenta>>(await _tipoDocumentoVentaServicio.Lista());
            return StatusCode(StatusCodes.Status200OK, vmLista);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerProductos(string busqueda)
        {
            List<VMProducto> vmLista = _mapper.Map<List<VMProducto>>(await _ventaServicio.ObtenerProductos(busqueda));
            return StatusCode(StatusCodes.Status200OK, vmLista);
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarVenta([FromBody] VMVenta modelo)
        {   
            GenericResponse<VMVenta> gResponse = new GenericResponse<VMVenta>();
            try
            {
                ClaimsPrincipal claimUser = HttpContext.User;
                string idUsuario = claimUser.Claims
                    .Where(c => c.Type == ClaimTypes.NameIdentifier)
                    .Select(c => c.Value).SingleOrDefault();
                modelo.IdUsuario = idUsuario != null ? int.Parse(idUsuario) : 1;

                Venta venta_creada = await _ventaServicio.Registrar(_mapper.Map<Venta>(modelo));
                modelo = _mapper.Map<VMVenta>(venta_creada);

                gResponse.Estado = true;
                gResponse.Objeto = modelo;
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpPost]
        public async Task<IActionResult> GuardarDatosFactura([FromBody] VMDatosFactura modelo)
        {
            //if (modelo == null)
              //  return BadRequest(new { estado = false, mensaje = "El modelo llegó null al servidor" });

            GenericResponse<VMVenta> gResponse = new GenericResponse<VMVenta>();
            try
            {
                Venta venta = await _ventaServicio.GuardarDatosFactura(
                    modelo.NumeroVenta,
                    modelo.UsoCFDI,
                    modelo.RegimenFiscal,
                    modelo.FormaPago,
                    modelo.MetodoPago,
                    modelo.TipoComprobante
                );
                gResponse.Estado = true;
                gResponse.Objeto = _mapper.Map<VMVenta>(venta);
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpGet]
        public async Task<IActionResult> Historial(string numeroVenta, string fechaInicio, string fechaFin)
        {
            List<VMVenta> vmLista = _mapper.Map<List<VMVenta>>(await _ventaServicio.Historial(numeroVenta, fechaInicio, fechaFin));
            return StatusCode(StatusCodes.Status200OK, vmLista);
        }

        public IActionResult MostrarPDFVenta(string numeroVenta)
        {
            string urlPlantillaVista = $"{this.Request.Scheme}://{this.Request.Host}/Plantilla/PDFVenta?numeroVenta={numeroVenta}";
            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = new GlobalSettings()
                {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Portrait,
                },
                Objects = { new ObjectSettings() { Page = urlPlantillaVista } }
            };
            var archivoPDF = _converter.Convert(pdf);
            return File(archivoPDF, "application/pdf");
        }
    }
}