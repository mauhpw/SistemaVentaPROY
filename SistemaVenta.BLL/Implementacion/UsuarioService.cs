using System.Text;

using Microsoft.EntityFrameworkCore;
using System.Net;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity.Models;

namespace SistemaVenta.BLL.Implementacion
{
    public class UsuarioService : IUsuarioService
    {


        private readonly IGenericRepository<Usuario> _repositorio;
        private readonly IFireBaseService _fireBaseService;
        private readonly IUtilidadesService _utilidadesService;
        private readonly ICorreoService _correoService;

        public UsuarioService(
        IGenericRepository<Usuario> repositorio,
        IFireBaseService fireBaseService,
        IUtilidadesService utilidadesService,
        ICorreoService correoService
            )
        {
            _repositorio = repositorio;
            _fireBaseService = fireBaseService;
            _utilidadesService = utilidadesService;
            _correoService = correoService;
            
        }

        public async Task<List<Usuario>> Lista()
        {
            IQueryable<Usuario> query = await _repositorio.Consultar();
                return query.Include(r => r.IdRolNavigation).ToList();
        }

        public async Task<Usuario> Crear(Usuario entidad, Stream Foto = null, string NombreFoto = "", string UrlPlantillaCorreo = "")
        {
            Usuario usuario_existe = await _repositorio.Obtener(u => u.Correo == entidad.Correo);

            if (usuario_existe != null)
                throw new TaskCanceledException("El correo ya existe");

            try
            {
                string clave_generada = _utilidadesService.GenerarClave();
                entidad.Clave = _utilidadesService.ConvertirSha256(clave_generada);
                entidad.NombreFoto = NombreFoto;

                if (Foto != null)
                {
                    string urlFoto = await _fireBaseService.SubirStorage(Foto, "carpeta_usuario", NombreFoto);
                    entidad.UrlFoto = urlFoto;
                }

                Usuario usuario_creado = await _repositorio.Crear(entidad);

                // --- CORRECCIÓN 1: La lógica estaba al revés ---
                // Si el ID es 0, significa que NO se creó. Solo ahí lanzamos error.
                if (usuario_creado.IdUsuario == 0)
                    throw new TaskCanceledException("No se pudo crear el usuario");

                // --- CORRECCIÓN 2: Blindamos el correo ---
                // Si el correo falla, NO importa, el usuario ya existe.
                if (UrlPlantillaCorreo != "")
                {
                    try
                    {
                        UrlPlantillaCorreo = UrlPlantillaCorreo
                            .Replace("[correo]", usuario_creado.Correo)
                            .Replace("[clave]", clave_generada);

                        string htmlCorreo = "";

                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlPlantillaCorreo);
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            using (Stream dataStream = response.GetResponseStream())
                            {
                                StreamReader readerStream = null;

                                if (response.CharacterSet == null)
                                    readerStream = new StreamReader(dataStream);
                                else
                                    readerStream = new StreamReader(dataStream, Encoding.GetEncoding(response.CharacterSet));

                                htmlCorreo = readerStream.ReadToEnd();
                                response.Close();
                                readerStream.Close();
                            }
                        }

                        if (htmlCorreo != "")
                        {
                            await _correoService.EnviarCorreo(usuario_creado.Correo, "Cuenta creada", htmlCorreo);
                        }
                    }
                    catch
                    {
                        // Si falla el correo, no hacemos nada.
                        // El usuario ya se creó, así que continuamos felices.
                    }
                }

                // Recuperamos el usuario con su Rol para que la tabla se vea bonita
                IQueryable<Usuario> query = await _repositorio.Consultar(u => u.IdUsuario == usuario_creado.IdUsuario);
                usuario_creado = query.Include(r => r.IdRolNavigation).First();

                return usuario_creado;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<Usuario> Editar(Usuario entidad, Stream Foto = null, string NombreFoto = "")
        {
            Usuario usuario_existe = await _repositorio.Obtener(u => u.Correo == entidad.Correo && u.IdUsuario != entidad.IdUsuario);

            if (usuario_existe != null)
                throw new TaskCanceledException("El correo ya existe");

            try
            {
                IQueryable<Usuario> queryUsuario = await _repositorio.Consultar(u => u.IdUsuario == entidad.IdUsuario);
                Usuario usuario_editar = queryUsuario.First();

                usuario_editar.Nombre = entidad.Nombre;
                usuario_editar.Correo = entidad.Correo;
                usuario_editar.Telefono = entidad.Telefono;
                usuario_editar.IdRol = entidad.IdRol;

                if (usuario_editar.NombreFoto == "")
                    usuario_editar.NombreFoto = NombreFoto;

                if (Foto != null)
                {
                    string urlFoto = await _fireBaseService.SubirStorage(Foto, "carpeta_usuario", usuario_editar.NombreFoto);
                    usuario_editar.UrlFoto = urlFoto;
                }

                bool respuesta = await _repositorio.Editar(usuario_editar);

                if (!respuesta)
                    throw new TaskCanceledException("No se pudo editar el usuario");

                Usuario usuario_editado = queryUsuario.Include(r => r.IdRolNavigation).First();

                return usuario_editado;
            }

            catch {
                throw;
                }

            }


        public async Task<bool> Eliminar(int IdUsuario)
        {
            try
            {
                Usuario usuario_encontrado = await _repositorio.Obtener(u => u.IdUsuario == IdUsuario);

                if (usuario_encontrado == null)
                    throw new TaskCanceledException("El usuario no existe");

                string nombreFoto = usuario_encontrado.NombreFoto;

                bool respuesta = await _repositorio.Eliminar(usuario_encontrado);

                if (respuesta)
                {

                    try
                    {

                        await _fireBaseService.EliminarStorage("carpeta_usuario", nombreFoto);
                    }
                    catch
                    {
                    }
                }

                return true;
            }
            catch
            {
                throw;
            }
        }


        public async Task<Usuario> ObtenerPorCredenciales(string correo, string clave)
        {
            string clave_encriptada = _utilidadesService.ConvertirSha256(clave);

            Usuario usuario_encontrado = await _repositorio.Obtener(u => u.Correo.Equals(correo)
            && u.Clave.Equals(clave_encriptada));

            return usuario_encontrado;
        }

        public async Task<Usuario> ObtenerPorId(int IdUsuario)
        {
            IQueryable<Usuario> query = await _repositorio.Consultar(u => u.IdUsuario == IdUsuario);

            Usuario resultado = query.Include(r => r.IdRolNavigation).FirstOrDefault();

            return resultado;
        }
        public async Task<bool> GuardarPerfil(Usuario entidad)
        {
            try
            {
                Usuario usuario_encontrado = await _repositorio.Obtener(u => u.IdUsuario == entidad.IdUsuario); 

                if(usuario_encontrado == null)
                    throw new TaskCanceledException("El usuario no existe");

                usuario_encontrado.Correo = entidad.Correo;
                usuario_encontrado.Telefono = entidad.Telefono; 

                bool respuesta = await _repositorio.Editar(usuario_encontrado);

                return respuesta;

            }
            catch {
                throw;
            
            }
        } 

        public async Task<bool> CambiarClave(int IdUsuario, string ClaveActual, string ClaveNueva)
        {
            try
            {
                Usuario usuario_encontrado = await _repositorio.Obtener(u => u.IdUsuario == IdUsuario);

                if(usuario_encontrado == null)
                    throw new TaskCanceledException("El usuario no existe");

                if(usuario_encontrado.Clave != _utilidadesService.ConvertirSha256(ClaveActual))
                    throw new TaskCanceledException("La clave actual es incorrecta");

                usuario_encontrado.Clave = _utilidadesService.ConvertirSha256(ClaveNueva);

                bool respuesta = await _repositorio.Editar(usuario_encontrado);

                return (respuesta);
            }
            catch(Exception ex) 
            {
                throw;
            }
        }



        public async Task<bool> RestablecerClave(string Correo, string UrlPlantillaCorreo)
        {
            try
            {
                Usuario usuario_encontrado = await _repositorio.Obtener(u => u.Correo == Correo);

                if (usuario_encontrado == null)
                    throw new TaskCanceledException("No encontramos ningun usuario asociado al correo");

                // 1. Generamos la clave nueva
                string clave_generada = _utilidadesService.GenerarClave();
                usuario_encontrado.Clave = _utilidadesService.ConvertirSha256(clave_generada);

                // 2. Preparamos el HTML
                UrlPlantillaCorreo = UrlPlantillaCorreo.Replace("[clave]", clave_generada);
                string htmlCorreo = "";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlPlantillaCorreo);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        StreamReader readerStream = null;

                        if (response.CharacterSet == null)
                            readerStream = new StreamReader(dataStream);
                        else
                            readerStream = new StreamReader(dataStream, Encoding.GetEncoding(response.CharacterSet));

                        htmlCorreo = readerStream.ReadToEnd();
                        response.Close();
                        readerStream.Close();
                    }
                }

                bool correo_enviado = false;

                if (htmlCorreo != "")
                    correo_enviado = await _correoService.EnviarCorreo(Correo, "Contraseña Restablecida", htmlCorreo);

                // --- AQUÍ ESTABA EL ERROR ---
                // Antes decías: Si se envió, lanza error.
                // CORRECCIÓN: Si NO (!) se envió, lanzamos error.
                if (!correo_enviado)
                {
                    throw new TaskCanceledException("Tenemos problemas. Por favor inténtalo de nuevo más tarde");
                }

                // 3. Si llegamos aquí, el correo se envió, AHORA SÍ GUARDAMOS EN LA BD
                bool respuesta = await _repositorio.Editar(usuario_encontrado);

                return respuesta;
            }
            catch
            {
                throw;
            }
        }

            }
        }

