using Microsoft.AspNetCore.Hosting;
using SistemaVenta.BLL.Interfaces;
using System.IO;
using System.Threading.Tasks;

namespace SistemaVenta.BLL.Implementacion
{
    public class ArchivoLocalService : IFireBaseService
    {
        private readonly IWebHostEnvironment _env;

        public ArchivoLocalService(IWebHostEnvironment env)
        {
            _env = env;
        }

        // Esta firma coincide exactamente con tu interfaz actual
        public async Task<string> SubirStorage(Stream StreamArchivo, string CarpetaDestino, string NombreArchivo)
        {
            // CarpetaDestino suele ser "carpeta_producto", "carpeta_usuario", etc.
            // Creamos la ruta física
            string rutaDirectorio = Path.Combine(_env.WebRootPath, "imagenes", CarpetaDestino);

            if (!Directory.Exists(rutaDirectorio))
                Directory.CreateDirectory(rutaDirectorio);

            string rutaCompleta = Path.Combine(rutaDirectorio, NombreArchivo);

            using (var fileStream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await StreamArchivo.CopyToAsync(fileStream);
            }

            // Retornamos la ruta relativa para guardar en la BD
            return $"/imagenes/{CarpetaDestino}/{NombreArchivo}";
        }

        public async Task<bool> EliminarStorage(string CarpetaDestino, string NombreArchivo)
        {
            string rutaCompleta = Path.Combine(_env.WebRootPath, "imagenes", CarpetaDestino, NombreArchivo);
            if (File.Exists(rutaCompleta))
            {
                File.Delete(rutaCompleta);
                return true;
            }
            return false;
        }
    }
}