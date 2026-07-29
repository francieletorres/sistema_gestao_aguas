using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WaterManagementSystem.Api.Controllers
{
    public class ConsumptionsController : ApiController
    {
        WaterManagementDataContext dc = new WaterManagementDataContext(ConfigurationManager.ConnectionStrings["WaterManagementSystemDBConnectionString"].ConnectionString);
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
                           consumption.Notes,
                           HasInvoice = consumption.Invoices.Any(),
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

        [HttpGet]
        [Route("api/consumptions/meter/{meterId}")]
        public IHttpActionResult GetConsumptionByMeter(int meterId)
        {
            var consumptions = dc.Consumptions.Where(c => c.MeterId == meterId)
                .Select(c => new
                {
                    c.ConsumptionId,
                    c.MeterId,
                    CustomerName = c.Meter.Customer.Name,
                    c.MeterReading,
                    c.ReadingDate,
                    c.ConsumedVolume,
                    c.Notes,
                    HasInvoice = c.Invoices.Any()
                })
            .ToList();

            //if(consumptions.Count == 0)
            //{
            //    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "No consumption has been registered for this meter yet"));

            //}
            //return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, consumptions.ToList()));

            return Ok(consumptions);
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

            if (!CanRegisterConsumption(meter))
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Inactive meters or customers cannot register new consumptions."));
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

            // Busca a leitura anterior do contador e calcula o volume consumido
            newConsumption.ConsumedVolume = CalculateConsumedVolume(newConsumption.MeterId, newConsumption.MeterReading, newConsumption.ReadingDate);


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

        private bool CanRegisterConsumption(Meter meter)
        {
            if (meter.IsActive == true && meter.Customer.IsActive == true)
            {
                return true;
            }

            return false;
        }

        private decimal CalculateConsumedVolume(int meterId, int meterReading, DateTime readingDate)
        {
            Consumption lastConsumption = dc.Consumptions.Where(c => c.MeterId == meterId && c.ReadingDate < readingDate)
           .OrderByDescending(c => c.ReadingDate).FirstOrDefault();

            if (lastConsumption != null)
            {
                return meterReading - lastConsumption.MeterReading;
            }

            return meterReading;
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

            if (consumption == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Consumption not found."));
            }

            if (consumption.Invoices.Any())
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "This consumption has already been invoiced and cannot be changed."));

            }
                Meter meter = dc.Meters.FirstOrDefault(m => m.MeterId == updateConsumption.MeterId);

            if (meter == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Meter not found."));
            }

            if (!CanRegisterConsumption(meter))
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Inactive meters or customers cannot register new consumptions."));
            }

            //leitura nao pode ser negativa
            if (updateConsumption.MeterReading < 0)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "Meter reading cannot be negative."));
            }

            //verifica se existe outra leitura para esse mesmo meterId na mesma data
            bool readingAlreadyExists = dc.Consumptions.Any(c => c.MeterId == updateConsumption.MeterId
            && c.ReadingDate == updateConsumption.ReadingDate && c.ConsumptionId != id);

            if (readingAlreadyExists)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "A reading already exists for this meter on this date."));
            }

            // Busca a leitura anterior, ignora o próprio consumo editado e calcula o volume consumido
            updateConsumption.ConsumedVolume = CalculateConsumedVolumeForEdit(updateConsumption.MeterId, updateConsumption.MeterReading, updateConsumption.ReadingDate, id);

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


        private decimal CalculateConsumedVolumeForEdit(int meterId, int meterReading, DateTime readingDate, int consumptionId)
        {
            Consumption lastConsumption = dc.Consumptions.Where(c => c.MeterId == meterId && c.ReadingDate < readingDate && c.ConsumptionId != consumptionId)
                .OrderByDescending(c => c.ReadingDate).FirstOrDefault();

            if (lastConsumption != null)
            {
                return meterReading - lastConsumption.MeterReading;
            }

            return meterReading;
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
