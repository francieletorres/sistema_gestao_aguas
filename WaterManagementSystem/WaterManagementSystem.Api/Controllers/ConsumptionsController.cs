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

        /// <summary>
        /// Gets all consumptions registered in the system.
        /// </summary>
        /// <returns>The list of consumptions.</returns>
        // GET: api/Consumptions
        public IHttpActionResult Get()
        {
            var list = from consumption
                     in dc.Consumptions
                       select new 
                       {
                           consumption.ConsumptionId,
                           consumption.MeterId,
                           CustomerName = consumption.Meter.Customer.Name,
                           consumption.MeterReading,
                           consumption.ReadingDate,
                           consumption.ConsumedVolume,
                           consumption.Notes,
                           HasInvoice = consumption.Invoices.Any(i => i.IsCancelled == false),
                       };
            
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, list.ToList()));
        }

        // GET: api/Consumptions/5
        /// <summary>
        /// Gets a consumption by identifier.
        /// </summary>
        /// <param name="id">The consumption identifier</param>
        /// <returns>The consumption data or a not found response.</returns>
        public IHttpActionResult Get(int id)
        {
            var consumption = dc.Consumptions.SingleOrDefault(c => c.ConsumptionId == id);

            if (consumption != null)
            {
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

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Consumo não encontrado."));

        }


        /// <summary>
        /// Gets all consumptions associated with a specific meter.
        /// </summary>
        /// <param name="meterId">The meter identifier.</param>
        /// <returns>The meter consumptions or a not found response.</returns>
        [HttpGet]
        [Route("api/consumptions/meter/{meterId}")]
        public IHttpActionResult GetConsumptionByMeter(int meterId)
        {
            CheckEstimatedReading(meterId);

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
                    HasInvoice = c.Invoices.Any(i => i.IsCancelled == false)
                })
            .ToList();

            if (consumptions.Count == 0)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Este contador ainda não possui consumos registados."));

            }
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, consumptions));

            //return Ok(consumptions);
        }

        // POST: api/Consumptions
        /// <summary>
        /// Creates a new consumption record for an active meter and customer.
        /// </summary>
        /// <param name="newConsumption">The consumption data to be created.</param>
        /// <returns>The result of the consumption creation operation.</returns>
        public IHttpActionResult Post([FromBody] Consumption newConsumption)
        {

            if (newConsumption == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "Dados do consumo inválidos."));
            }

            // nao pode inserir leitura para o futuro
            if (newConsumption.ReadingDate.Date > DateTime.Now.Date)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "A data da leitura não pode ser futura."));
            }

            Meter meter = dc.Meters.FirstOrDefault(m => m.MeterId == newConsumption.MeterId);

            //para nao criar um consumo com contador vazio
            if (meter == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Contador não encontrado."));
            }

            if (!CanRegisterConsumption(meter))
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Não é possível registar novos consumos para clientes ou contadores inativos."));
            }

            //leitura nao pode ser negativa
            if (newConsumption.MeterReading < 0)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "A leitura não pode ser negativa."));
            }

            //verifica se existe uma leitura para esse mesmo meterId na mesma data

            DateTime firstDayOfMonth = new DateTime(newConsumption.ReadingDate.Year,newConsumption.ReadingDate.Month,1);

            DateTime firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);

            bool readingAlreadyExists = dc.Consumptions.Any(c => c.MeterId == newConsumption.MeterId && c.ReadingDate >= firstDayOfMonth && c.ReadingDate < firstDayOfNextMonth);

            if (readingAlreadyExists)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Já existe uma leitura para este contador este mês."));
            }

            // Busca a leitura anterior do contador e calcula o volume consumido
            newConsumption.ConsumedVolume = CalculateConsumedVolume(newConsumption.MeterId, newConsumption.MeterReading, newConsumption.ReadingDate);


            //volume consumido nao pode ser negativo
            if (newConsumption.ConsumedVolume < 0)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "O consumo não pode ser negativo."));

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

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.Created, "Consumo criado com sucesso."));
        }

        /// <summary>
        /// Checks whether the meter and its customer are active.
        /// </summary>
        /// <param name="meter">The meter to be checked.</param>
        /// <returns>Returns true when both the meter and customer are active; otherwise, false.</returns>
        private bool CanRegisterConsumption(Meter meter)
        {
            if (meter.IsActive == true && meter.Customer.IsActive == true)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Calculates the consumed volume based on the previous meter reading.
        /// </summary>
        /// <returns>Returns the calculated consumed volume.</returns>
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
        /// <summary>
        ///  Updates an existing consumption record when it has not been invoiced.
        /// </summary>
        /// <param name="id">The consumption identifier.</param>
        /// <returns>The result of the consumption update operation.</returns>
        public IHttpActionResult Put(int id, [FromBody] Consumption updateConsumption)
        {
            if (updateConsumption == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "Dados de consumo inválidos."));
            }

            if (updateConsumption.ReadingDate.Date > DateTime.Now.Date)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "A data da leitura não pode ser futura."));
            }

            //buscar o id da requisição
            Consumption consumption = dc.Consumptions.FirstOrDefault(c => c.ConsumptionId == id);

            if (consumption == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Consumo não encontrado."));
            }

            if (consumption.Invoices.Any(i => i.IsCancelled == false))
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Consumo faturado não pode ser alterado."));

            }
                Meter meter = dc.Meters.FirstOrDefault(m => m.MeterId == updateConsumption.MeterId);

            if (meter == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Contador não encontrado."));
            }

            if (!CanRegisterConsumption(meter))
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Não é possível registar novos consumos para clientes ou contadores inativos."));
            }

            //leitura nao pode ser negativa
            if (updateConsumption.MeterReading < 0)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "A leitura não pode ser negativa."));
            }

            //verifica se existe outra leitura para esse mesmo meterId na mesma data
            bool readingAlreadyExists = dc.Consumptions.Any(c => c.MeterId == updateConsumption.MeterId
            && c.ReadingDate == updateConsumption.ReadingDate && c.ConsumptionId != id);

            if (readingAlreadyExists)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Já existe uma leitura para este contador nesta data."));
            }

            // Busca a leitura anterior, ignora o próprio consumo editado e calcula o volume consumido
            updateConsumption.ConsumedVolume = CalculateConsumedVolumeForEdit(updateConsumption.MeterId, updateConsumption.MeterReading, updateConsumption.ReadingDate, id);

            //volume consumido nao pode ser negativo
            if (updateConsumption.ConsumedVolume < 0)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "O consumo não pode ser negativo."));

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

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, "Consumo atualizado com sucesso."));
        }


        /// <summary>
        /// Calculates the consumed volume during an update,
        /// excluding the consumption being edited.
        /// </summary>
        /// <returns>Returns the calculated consumed volume.</returns>
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
        /// <summary>
        /// Delete an existing consumption when no invoice is associated with it.
        /// </summary>
        /// <param name="id">The consumption identifier.</param>
        /// <returns>The result of the consumption deletion operation.</returns>
        public IHttpActionResult Delete(int id)
        {
            Consumption consumption = dc.Consumptions.FirstOrDefault(c => c.ConsumptionId == id);


            if (consumption == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Consumo não encontrado."));
            }

            bool consumptionHasInvoice = dc.Invoices.Any(i => i.ConsumptionId == id);

            if (consumptionHasInvoice)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Não é possível apagar o consumo porque existe uma fatura associada."));
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

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, "Consumo apagado com sucesso."));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="meterId"></param>
        private void CheckEstimatedReading(int meterId)
        {
            int limitDay = 25;

            DateTime today = DateTime.Now;

            // A leitura estimada só é criada a partir do dia 25.
            if (today.Day < limitDay)
            {
                return;
            }

            Meter meter = dc.Meters
                .SingleOrDefault(m => m.MeterId == meterId);

            // Verifica se o contador existe e está ativo.
            if (meter == null || meter.IsActive == false)
            {
                return;
            }

            // Verifica se o cliente do contador está ativo.
            if (meter.Customer == null || meter.Customer.IsActive == false)
            {
                return;
            }

            DateTime firstDayOfMonth =
                new DateTime(today.Year, today.Month, 1);

            DateTime firstDayOfNextMonth =
                firstDayOfMonth.AddMonths(1);

            // Verifica se o contador já possui uma leitura neste mês.
            bool alreadyHasReading = dc.Consumptions.Any(c =>
                c.MeterId == meterId &&
                c.ReadingDate >= firstDayOfMonth &&
                c.ReadingDate < firstDayOfNextMonth);

            if (alreadyHasReading)
            {
                return;
            }

            var previousConsumptions = dc.Consumptions
                .Where(c => c.MeterId == meterId)
                .OrderBy(c => c.ReadingDate)
                .ToList();

            decimal averageConsumption;
            decimal lastReading;

            if (previousConsumptions.Count > 0)
            {
                averageConsumption =
                    previousConsumptions.Average(c => c.ConsumedVolume);

                lastReading = previousConsumptions
                    .OrderByDescending(c => c.ReadingDate)
                    .First()
                    .MeterReading;
            }
            else
            {
                // Valor utilizado quando o contador ainda não possui consumos.
                averageConsumption = 5;
                lastReading = 0;
            }

            Consumption estimatedConsumption = new Consumption
            {
                MeterId = meterId,
                MeterReading = Convert.ToInt32(lastReading + averageConsumption),
                ReadingDate = today,
                ConsumedVolume = averageConsumption,
                Notes = "Leitura estimada",
            };

            dc.Consumptions.InsertOnSubmit(estimatedConsumption);
            dc.SubmitChanges();
        }
    }
}
