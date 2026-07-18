using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WaterManagementSystem.Api.Controllers
{
    public class InvoicesController : ApiController
    {
        WaterManagementDataContext dc = new WaterManagementDataContext(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=WaterManagementSystemDB;Integrated Security=True");

        // GET: api/Invoices
        public IHttpActionResult Get()
        {
            var list = from invoice
                       in dc.Invoices
                       select new
                       {
                           invoice.InvoiceId,
                           invoice.ConsumptionId,
                           invoiceName = invoice.Consumption.Meter.Customer.Name,
                           invoice.IssueDate,
                           invoice.InvoiceAmount,
                           invoice.IsCancelled,
                           invoice.IsPaid
                       };

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, list.ToList()));
        }

        // GET: api/Invoices/5
        public IHttpActionResult Get(int id)
        {
            var invoice = dc.Invoices.FirstOrDefault(i => i.InvoiceId == id);

            //cria um obj temporário para nao gerar loop
            if (invoice != null)
            {
                var invoiceData = new
                {
                    invoice.InvoiceId,
                    invoice.ConsumptionId,
                    invoiceName = invoice.Consumption.Meter.Customer.Name,
                    invoice.IssueDate,
                    invoice.InvoiceAmount,
                    invoice.IsCancelled,
                    invoice.IsPaid

                };

                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, invoiceData));
            }
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Invoice not found"));
        }

        // POST: api/Invoices
        public IHttpActionResult Post([FromBody] Invoice newInvoice)
        {

            if (newInvoice == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid invoice data."));
            }

            //procurar o consumo pelo consumoid
            Consumption consumption = dc.Consumptions.FirstOrDefault(c => c.ConsumptionId == newInvoice.ConsumptionId);

            if (consumption == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Comsunption not found."));
            }

            //o any é um bool que confere se tem fatura 
            bool hasInvoice = dc.Invoices.Any(i => i.ConsumptionId == newInvoice.ConsumptionId && i.IsCancelled == false);

            if (hasInvoice)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "An invoice already exists for this consumption."));
            }

            //buscar o volume consumido do consumo encontrado
            decimal consumedVolume = consumption.ConsumedVolume;

            //calculo da fatura usando outro método
            decimal invoiceAmount = CalculateInvoiceAmount(consumedVolume);

            newInvoice.InvoiceAmount = invoiceAmount;
            newInvoice.IssueDate = DateTime.Now;
            newInvoice.IsPaid = false;
            newInvoice.IsCancelled = false;

            dc.Invoices.InsertOnSubmit(newInvoice);

            try
            {
                dc.SubmitChanges();
            }
            catch (Exception e)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.ServiceUnavailable, e));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.Created, "Invoice created successfully."));

        }

        private decimal CalculateInvoiceAmount(decimal consumedVolume)
        {
            decimal invoiceAmount = 0;
            decimal remainingVolume = consumedVolume;

            decimal firstTierRate = Convert.ToDecimal(0.30);
            decimal secondTierRate = Convert.ToDecimal(0.80);
            decimal thirdTierRate = Convert.ToDecimal(1.20);
            decimal fourthTierRate = Convert.ToDecimal(1.60);

            // 1 escalao ate 5m cubicos (0,30)
            if (remainingVolume > 5)
            {
                invoiceAmount += 5 * firstTierRate;
                remainingVolume -= 5;
            }
            else
            {
                invoiceAmount += remainingVolume * firstTierRate;
                remainingVolume = 0;
            }

            //2 escalao superior a 5 até 15 (0.80)
            if (remainingVolume > 10)
            {
                invoiceAmount += 10 * secondTierRate;
                remainingVolume -= 10;
            }
            else
            {
                invoiceAmount += remainingVolume * secondTierRate;
                remainingVolume = 0;
            }

            //3 escalao superior a 15 até 25 (1.20)
            if (remainingVolume > 10)
            {
                invoiceAmount += 10 * thirdTierRate;
                remainingVolume -= 10;
            }
            else
            {
                invoiceAmount += remainingVolume * thirdTierRate;
                remainingVolume = 0;
            }

            //4 escalao superior a 25 (1.60)
            if (remainingVolume > 0)
            {
                invoiceAmount += remainingVolume * fourthTierRate;
            }

            return Math.Round(invoiceAmount, 2);
        }

        // PUT: api/Invoices/5
        public IHttpActionResult Put(int id, [FromBody] Invoice updateInvoice)
        {
            if (updateInvoice == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid invoice data"));
            }

            Invoice invoice = dc.Invoices.FirstOrDefault(i => i.InvoiceId == id);

            if (invoice == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Invoice not found."));
            }

            //nao pode pagar e cancelar ao mesmo tempo
            if(updateInvoice.IsCancelled == true && updateInvoice.IsPaid == true)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "Invoice cannot be paid and cancelled at the same time."));
            }

            //fatura cancelada nao pode ser editada
            if(invoice.IsCancelled == true)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Cancelled invoices cannot be updated."));
            }

            //se está cancelada o ispaid deve ficar falso
            if (updateInvoice.IsCancelled)
            {
                invoice.IsCancelled = true;
                invoice.IsPaid = false;
            }
            else
            {
                invoice.IsCancelled = false;
                invoice.IsPaid = updateInvoice.IsPaid;
            }
            
            try
            {
                dc.SubmitChanges();
            }
            catch (Exception e)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.ServiceUnavailable, e));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, "Invoice updated successfully."));

        }

        // DELETE: api/Invoices/5
        public IHttpActionResult Delete(int id)
        {
            Invoice invoice = dc.Invoices.FirstOrDefault(i => i.InvoiceId == id);

            if(invoice == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Invoice not found."));
            }

            if (invoice.IsPaid == true)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Paid invoices cannot be deleted."));
            }

            if (invoice.IsCancelled == true)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Cancelled invoices cannot be deleted."));
            }


            dc.Invoices.DeleteOnSubmit(invoice);
            try
            {
                dc.SubmitChanges();
            }
            catch (Exception e)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.ServiceUnavailable, e));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, "Invoice deleted successfully."));

        }
    }
}
