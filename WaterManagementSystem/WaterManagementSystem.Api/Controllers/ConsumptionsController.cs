using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WaterManagementSystem.Api.Controllers
{
    public class ConsumptionsController : ApiController
    {
        WaterManagementDataContext dc = new WaterManagementDataContext(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=WaterManagementSystemDB;Integrated Security=True");
        // GET: api/Consumptions
        public IHttpActionResult Get()
        {
            var list = from consumption
                     in dc.Consumptions
                       select new  //obj tem loop
                       {
                           consumption.ConsumptionId,
                           consumption.MeterId,
                           CustomerName = consumption.Meter.Customer.Name,
                           consumption.MeterReading,
                           consumption.ReadingDate,
                           consumption.ConsumedVolume,
                           consumption.Notes
                       };

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, list.ToList()));
        }

        // GET: api/Consumptions/5
        public IHttpActionResult Get(int id)
        {
            var consumption = dc.Consumptions.SingleOrDefault(c => c.ConsumptionId == id);

            if (consumption != null)
            {

                //cria um obj temporario
                var consumptionData = new
                {
                    consumption.ConsumptionId,
                    consumption.MeterId,
                    CustomerName = consumption.Meter.Customer.Name,
                    consumption.MeterReading,
                    consumption.ReadingDate,
                    consumption.ConsumedVolume,
                    consumption.Notes
                };

                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, consumptionData));

            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Consumption not found."));

        }

        // POST: api/Consumptions
        public IHttpActionResult Post([FromBody] Consumption newConsumption)
        {

            if (newConsumption == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid consumption data."));
            }

            // nao pode inserir leitura para o futuro
            if (newConsumption.ReadingDate.Date > DateTime.Now.Date)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "Reading date cannot be in the future."));
            }

            Meter meter = dc.Meters.FirstOrDefault(m => m.MeterId == newConsumption.MeterId);

            //para nao criar um consumo com contador vazio
            if (meter == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Meter not found."));
            }

            if (!meter.IsActive)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Inactive meters cannot register new consumptions."));
            }

            if (!meter.Customer.IsActive)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Inactive customers cannot register new consumptions."));
            }

            //leitura nao pode ser negativa
            if (newConsumption.MeterReading < 0)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "Meter reading cannot be negative."));
            }

            //verifica se existe uma leitura para esse mesmo meterId na mesma data
            bool readingAlreadyExists = dc.Consumptions.Any(c => c.MeterId == newConsumption.MeterId && c.ReadingDate == newConsumption.ReadingDate);

            if (readingAlreadyExists)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "A reading already exists for this meter on this date."));
            }

            //busca leitura anterior
            Consumption lastConsumption = dc.Consumptions.Where(c => c.MeterId == newConsumption.MeterId && c.ReadingDate < newConsumption.ReadingDate)
                .OrderByDescending(c => c.ReadingDate).FirstOrDefault();

            //calculo do volumeConsumido
            if (lastConsumption != null)
            {
                newConsumption.ConsumedVolume = newConsumption.MeterReading - lastConsumption.MeterReading;

            }
            else
            {
                newConsumption.ConsumedVolume = newConsumption.MeterReading;
            }

            //volume consumido nao pode ser negativo
            if (newConsumption.ConsumedVolume < 0)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "Consumption cannot be negative."));

            }

            dc.Consumptions.InsertOnSubmit(newConsumption);

            try
            {
                dc.SubmitChanges();
            }
            catch (Exception e)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.ServiceUnavailable, e));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.Created, "Consumption created successfully."));
        }

        // PUT: api/Consumptions/5
        public IHttpActionResult Put(int id, [FromBody] Consumption updateConsumption)
        {
            if (updateConsumption == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid consumption data."));
            }

            if (updateConsumption.ReadingDate.Date > DateTime.Now.Date)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "Reading date cannot be in the future."));
            }

            //buscar o id da requisição
            Consumption consumption = dc.Consumptions.FirstOrDefault(c => c.ConsumptionId == id);

            if(consumption == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Consumption not found."));
            }

            Meter meter = dc.Meters.FirstOrDefault(m => m.MeterId == updateConsumption.MeterId);

            if (meter == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Meter not found."));
            }

            if (!meter.IsActive)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Inactive meters cannot register new consumptions."));
            }

            if (!meter.Customer.IsActive)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Inactive customers cannot register new consumptions."));
            }

            //verifica se existe outra leitura para esse mesmo meterId na mesma data
            bool readingAlreadyExists = dc.Consumptions.Any(c => c.MeterId == updateConsumption.MeterId 
            && c.ReadingDate == updateConsumption.ReadingDate && c.ConsumptionId != id);

            if (readingAlreadyExists)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "A reading already exists for this meter on this date."));
            }

            //busca leitura anterior, mas ignora o próprio consumo editado
            Consumption lastConsumption = dc.Consumptions.Where(c => c.MeterId == updateConsumption.MeterId && c.ReadingDate < updateConsumption.ReadingDate &&
               c.ConsumptionId != id) .OrderByDescending(c => c.ReadingDate).FirstOrDefault();

            //calculo do volumeConsumido
            if (lastConsumption != null)
            {
                updateConsumption.ConsumedVolume = updateConsumption.MeterReading - lastConsumption.MeterReading;

            }
            else
            {
                updateConsumption.ConsumedVolume = updateConsumption.MeterReading;
            }

            //volume consumido nao pode ser negativo
            if (updateConsumption.ConsumedVolume < 0)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "Consumption cannot be negative."));

            }

            consumption.MeterId = updateConsumption.MeterId;
            consumption.MeterReading = updateConsumption.MeterReading;
            consumption.ReadingDate = updateConsumption.ReadingDate;
            consumption.ConsumedVolume = updateConsumption.ConsumedVolume;
            consumption.Notes = updateConsumption.Notes;

            try
            {
                dc.SubmitChanges();
            }
            catch (Exception e)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.ServiceUnavailable, e));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, "Consumption updated successfully."));
        }

        // DELETE: api/Consumptions/5
        public IHttpActionResult Delete(int id)
        {
            Consumption consumption = dc.Consumptions.FirstOrDefault(c => c.ConsumptionId == id);


            if (consumption == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Consumption not found"));
            }

            bool consumptionHasInvoice = dc.Invoices.Any(i => i.ConsumptionId == id);

           if (consumptionHasInvoice)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Consumption cannot be deleted because there is an invoice associated."));
            }

            dc.Consumptions.DeleteOnSubmit(consumption);

            try
            {
                dc.SubmitChanges();
            }
            catch (Exception e)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.ServiceUnavailable, e));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, "Consumption deleted successfully."));
        }
    }
}
