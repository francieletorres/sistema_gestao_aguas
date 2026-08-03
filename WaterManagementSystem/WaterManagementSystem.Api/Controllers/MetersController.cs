using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WaterManagementSystem.Api.Controllers
{
    public class MetersController : ApiController
    {
        WaterManagementDataContext dc = new WaterManagementDataContext(ConfigurationManager.ConnectionStrings["WaterManagementSystemDBConnectionString"].ConnectionString);

        // GET: api/Meters
        /// <summary>
        /// Gets all meters registered in the system.
        /// </summary>
        /// <returns>The list of meters.</returns>
        public IHttpActionResult Get()
        {
            var list = from meter
                       in dc.Meters
                       select new  
                       {
                           meter.MeterId,
                           meter.CustomerId,
                           CustomerName = meter.Customer.Name,
                           meter.InstallationDate,
                           meter.IsActive
                       };

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, list.ToList()));
        }

        // GET: api/Meters/5
        /// <summary>
        /// Gets a meter by identifier
        /// </summary>
        /// <param name="id">The meter identifier.</param>
        /// <returns>The meter data or a not found response.</returns>
        public IHttpActionResult Get(int id)
        {
            var meter = dc.Meters.SingleOrDefault(m => m.MeterId == id);

            if(meter != null)
            {
                //mostrar apenas alguns campos para nao dar o error 500
                var meterData = new
                {
                    meter.MeterId,
                    meter.CustomerId,
                    CustomerName = meter.Customer.Name,
                    meter.InstallationDate,
                    meter.IsActive
                };

                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, meterData));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Contador não encontrado."));
        }


        /// <summary>
        /// Gets all meters associated with a specific customer.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns>Returns the customer's meters or a not found response.</returns>
        [HttpGet]
        [Route("api/meters/customer/{customerId}")]
        public IHttpActionResult GetByCustomer(int customerId)
        {
            var meters = dc.Meters.Where(m => m.CustomerId == customerId)
                .Select(m => new
                {
                    m.MeterId,
                    m.CustomerId,
                    CustomerName = m.Customer.Name,
                    m.InstallationDate,
                    m.IsActive
                })
                .ToList();

            if (meters.Count == 0)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Este cliente ainda não possui contadores registados."));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, meters));

            //return Ok(meters);
        }


        // POST: api/Meters
        /// <summary>
        /// Creates a new meter for an existing customer.
        /// </summary>
        /// <param name="newMeter">The meter data to be created.</param>
        /// <returns>The result of the meter creation operation.</returns>
        public IHttpActionResult Post([FromBody] Meter newMeter)
        {
            if (newMeter == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "Dados do contador inválidos."));
            }

            // Finds the customer associated with the provided CustomerId.
            Customer customer = dc.Customers.SingleOrDefault(c => c.CustomerId == newMeter.CustomerId);       
            
            if(customer == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Cliente não encontrado."));
            }

            //newMeter.InstallationDate = DateTime.SpecifyKind(newMeter.InstallationDate.Date, DateTimeKind.Unspecified);

            newMeter.IsActive = true;

            dc.Meters.InsertOnSubmit(newMeter);

            try
            {
                dc.SubmitChanges();
            }
            catch(Exception e)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.ServiceUnavailable, e));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.Created, "Contador criado com sucesso."));
        }

        // PUT: api/Meters/5
        /// <summary>
        /// Updates the status of an existing meter.
        /// </summary>
        /// <param name="id">The meter identifier.</param>
        /// <param name="updateMeter">The new meter status.</param>
        /// <returns>The result of the meter update operation.</returns>
        public IHttpActionResult Put(int id, [FromBody]Meter updateMeter)
        {
            
            if(updateMeter == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "Dados do contador inválidos."));
            }

            Meter meter = dc.Meters.FirstOrDefault(m => m.MeterId == id);

            if(meter == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Contador não encontrado."));
            }

            meter.IsActive = updateMeter.IsActive;

            try
            {
                dc.SubmitChanges();
            }
            catch(Exception e)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.ServiceUnavailable, e));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, "Contador atualizado com sucesso."));
        }

        // DELETE: api/Meters/5
        /// <summary>
        /// Deletes an existing meter when no consumptions are associated with it.
        /// </summary>
        /// <param name="id">The meter identifier.</param>
        /// <returns>The result of the meter deletion operation.</returns>
        public IHttpActionResult Delete(int id)
        {

            Meter meter = dc.Meters.FirstOrDefault(m => m.MeterId == id);

            if(meter == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Contador não encontrado."));
            }

            bool meterHasConsumptions = dc.Consumptions.Any(c => c.MeterId == id);

            if (meterHasConsumptions)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Contador não pode ser apagado porque existem consumos associados."));
            }

            dc.Meters.DeleteOnSubmit(meter);

            try
            {
                dc.SubmitChanges();
            }
            catch(Exception e)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.ServiceUnavailable, e));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, "Contador apagado com sucesso."));
        }
    }
}
