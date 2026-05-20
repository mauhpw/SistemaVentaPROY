using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;

using System.Net;
using System.Net.Mail;

using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;
using SistemaVenta.Entity.Models;

namespace SistemaVenta.BLL.Implementacion
{
    public class CorreoService : ICorreoService
    {
        private readonly IGenericRepository<Configuracion> _repositorio;

        public CorreoService(IGenericRepository<Configuracion> repositorio)
        {
            _repositorio = repositorio;
        }
        public async Task<bool> EnviarCorreo(string CorreoDestino, string Asunto, string Mensaje)
        {
            try
            {
                IQueryable<Configuracion> query = await _repositorio.Consultar(c => c.Recurso.Equals("Servicio_Correo"));

                Dictionary<string, string> Config = query.ToDictionary(keySelector: c => c.Propiedad, elementSelector: c => c.Valor);

                var credenciales = new NetworkCredential(Config["Correo"], Config["Clave"]);

                var correo = new MailMessage()
                {
                    From = new MailAddress(Config["Correo"], Config["Alias"]),
                    Subject = Asunto,
                    Body = Mensaje,
                    IsBodyHtml = true

                };

                correo.To.Add(new MailAddress(CorreoDestino));

                var clienteServidor = new SmtpClient()
                {
                    Host = Config["Host"],
                    Port = int.Parse(Config["Puerto"]),
                    Credentials = credenciales,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    EnableSsl = true
                };

                clienteServidor.Send(correo);
                return true;
            }
            catch
            {
                return false;
            }
            throw new NotImplementedException();
        }
    }
}
