using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaVenta.AplicacionWeb.Models;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.Entity;
using System.Diagnostics;
using System.Security.Claims;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IUsuarioService _usuarioServicio;
        private readonly IFireBaseService _fireBaseService; // <-- Nuevo: Para la foto
        private readonly IMapper _mapper;

        public HomeController(IUsuarioService usuarioServicio, IFireBaseService fireBaseService, IMapper mapper)
        {
            _usuarioServicio = usuarioServicio;
            _fireBaseService = fireBaseService;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // 1. CARGAR PERFIL (GET)
        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            string idUsuario = claimUser.Claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier)
                .Select(c => c.Value).SingleOrDefault();

            // Buscamos al usuario real para llenar los campos
            var entidad = await _usuarioServicio.ObtenerPorId(int.Parse(idUsuario));
            // Lo convertimos al Modelo de la Vista
            VMUsuario modelo = _mapper.Map<VMUsuario>(entidad);

            return View(modelo);
        }

        // 2. GUARDAR DATOS Y FOTO (POST)
        [HttpPost]
        public async Task<IActionResult> GuardarPerfil(VMUsuario modelo, IFormFile? foto)
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            string idUsuario = claimUser.Claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier)
                .Select(c => c.Value).SingleOrDefault();

            // Obtenemos el usuario original de la BD para no perder datos
            var entidad = await _usuarioServicio.ObtenerPorId(int.Parse(idUsuario));

            // Actualizamos solo lo que se permite editar
            entidad.Nombre = modelo.Nombre;
            entidad.Telefono = modelo.Telefono;

            // Lógica de la Foto
            if (foto != null)
            {
                string nombre_en_codigo = Guid.NewGuid().ToString("N");
                string extension = Path.GetExtension(foto.FileName);
                string nombreFoto = string.Concat(nombre_en_codigo, extension);
                // Subimos la foto a Firebase
                string urlFoto = await _fireBaseService.SubirStorage(foto.OpenReadStream(), "carpeta_usuario", nombreFoto);
                // Actualizamos la entidad
                entidad.UrlFoto = urlFoto;
                entidad.NombreFoto = nombreFoto;
            }

            // Guardamos en la BD
            await _usuarioServicio.Editar(entidad);

            return RedirectToAction("Perfil");
        }

        // 3. CAMBIAR CONTRASEÑA (POST)
        [HttpPost]
        public async Task<IActionResult> CambiarClave(string claveActual, string claveNueva, string confirmarClave)
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            string idUsuario = claimUser.Claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier)
                .Select(c => c.Value).SingleOrDefault();

            if (claveNueva != confirmarClave)
            {
                TempData["ErrorClave"] = "Las contraseñas no coinciden";
                return RedirectToAction("Perfil");
            }

            bool resultado = await _usuarioServicio.CambiarClave(int.Parse(idUsuario), claveActual, claveNueva);

            if (resultado)
                TempData["MensajeClave"] = "Contraseña actualizada con éxito";
            else
                TempData["ErrorClave"] = "La contraseña actual es incorrecta";

            return RedirectToAction("Perfil");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Salir()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Acceso");
        }
    }
}