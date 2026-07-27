using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WaterManagementSystem.Api.Controllers
{
    public class MetersController : ApiController
    {
        WaterManagementDataContext dc = new WaterManagementDataContext(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=WaterManagementSystemDB;Integrated Security=True");

        // GET: api/Meters
        public IHttpActionResult Get()
        {
            var list = from meter
                       in dc.Meters
                       select new  //obj tem loop
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

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Not found"));
        }

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
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "No meter has been registered for this customer yet."));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK,meters));
        }

        // POST: api/Meters
        public IHttpActionResult Post([FromBody] Meter newMeter)
        {
            if (newMeter == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid meter data"));
            }

            //procuso um customerid que seja igual ao customerId que veio do meter
            Customer customer = dc.Customers.SingleOrDefault(c => c.CustomerId == newMeter.CustomerId);       
            
            //verifica se o cliente existe
            if(customer == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Customer not found"));
            }

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

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.Created, "Meter created successfully."));
        }

        // PUT: api/Meters/5
        public IHttpActionResult Put(int id, [FromBody]Meter updateMeter)
        {
            
            if(updateMeter == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid meter data!"));
            }

            Meter meter = dc.Meters.FirstOrDefault(m => m.MeterId == id);

            if(meter == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Meter not found!"));
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

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, "Meter updated successfully."));
        }

        // DELETE: api/Meters/5
        public IHttpActionResult Delete(int id)
        {

            Meter meter = dc.Meters.FirstOrDefault(m => m.MeterId == id);

            if(meter == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Meter not found"));
            }

            bool meterHasConsumptions = dc.Consumptions.Any(c => c.MeterId == id);

            if (meterHasConsumptions)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Meter cannot be deleted because there are consumptions associated."));
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

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, "Meter deleted successfully."));
        }
    }
}
