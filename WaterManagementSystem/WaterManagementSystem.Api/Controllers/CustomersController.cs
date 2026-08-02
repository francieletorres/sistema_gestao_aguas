using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Configuration;

namespace WaterManagementSystem.Api.Controllers
{
    public class CustomersController : ApiController
    {
        WaterManagementDataContext dc = new WaterManagementDataContext(ConfigurationManager.ConnectionStrings["WaterManagementSystemDBConnectionString"].ConnectionString);

        /// <summary>
        /// Gets all customers registered in the system.
        /// </summary>
        /// <returns>The list of customers.</returns>
        // GET: api/Customers
        public IHttpActionResult Get()
        {
            var list = from customer
                        in dc.Customers
                       select new 
                       {
                           customer.CustomerId,
                           customer.Name,
                           customer.Address,
                           customer.Phone,
                           customer.TaxNumber,
                           customer.Email,
                           customer.IsActive
                       };

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, list.ToList()));
        }

        // GET: api/Customers/5
        /// <summary>
        /// Gets a customer by identifier.
        /// </summary>
        /// <param name="id">The customer identifier.</param>
        /// <returns>The customer data or a not found response.</returns>
        public IHttpActionResult Get(int id)
        {
            var customer = dc.Customers.SingleOrDefault(c => c.CustomerId == id);

            if (customer != null)
            {
                var customerData = new
                {
                    customer.CustomerId,
                    customer.Name,
                    customer.Address,
                    customer.Phone,
                    customer.TaxNumber,
                    customer.Email,
                    customer.IsActive
                };

                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, customerData));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Customer not found"));
        }

        // POST: api/Customers
        /// <summary>
        ///  Creates a new customer.
        /// </summary>
        /// <param name="newCustomer">The customer data to be created.</param>
        /// <returns>The result of the customer creation operation.</returns>
        public IHttpActionResult Post([FromBody] Customer newCustomer)
        {
            if (newCustomer == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid customer data."));
            }

            //o any pergunta se existe alguém com esse nif. E responde true para existe e false nao existe
            bool customerExists = dc.Customers.Any(c => c.TaxNumber == newCustomer.TaxNumber);

            if (customerExists)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "A customer with this tax number already exists."));
            }

            newCustomer.IsActive = true;

            dc.Customers.InsertOnSubmit(newCustomer);

            try
            {
                dc.SubmitChanges();
            }
            catch (Exception e)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.ServiceUnavailable, e));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.Created, newCustomer));
        }

        // PUT: api/Customers/5
        /// <summary>
        /// Updates an existing customer.
        /// </summary>
        /// <param name="id">The customer identifier.</param>
        /// <param name="updateCustomer"></param>
        /// <returns>The result of the customer update operation.</returns>
        public IHttpActionResult Put(int id, [FromBody] Customer updateCustomer)
        {
            if (updateCustomer == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid customer data."));
            }

            Customer customer = dc.Customers.SingleOrDefault(c => c.CustomerId == id);

            if (customer == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Customer not found"));
            }

            bool anotherCustomerHasSameTaxNumber = dc.Customers.Any(c => (c.CustomerId != id && c.TaxNumber == updateCustomer.TaxNumber));

            if (anotherCustomerHasSameTaxNumber)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "A customer with this tax number already exists."));
            }

            customer.Name = updateCustomer.Name;
            customer.Address = updateCustomer.Address;
            customer.Phone = updateCustomer.Phone;
            customer.TaxNumber = updateCustomer.TaxNumber;
            customer.Email = updateCustomer.Email;
            customer.IsActive = updateCustomer.IsActive;

            try
            {
                dc.SubmitChanges();
            }
            catch (Exception e)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.ServiceUnavailable, e));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK));
        }

        // DELETE: api/Customers/5
        /// <summary>
        /// Delete an existing customer.
        /// </summary>
        /// <param name="id">The customer identifier.</param>
        /// <returns>The result of the customer deletion operation.</returns>
        public IHttpActionResult Delete(int id)
        {
            Customer customer = dc.Customers.SingleOrDefault(c => c.CustomerId == id);

            if (customer == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Customer not found"));
            }

            bool customerHasConsumptions = dc.Consumptions.Any
                (Consumption => Consumption.Meter.CustomerId == id);

            if (customerHasConsumptions)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, "Customer cannot be deleted because there are consumptions associated."));
            }

            //procura os contadores que pertecem ao cliente 
            var customerMeters = dc.Meters.Where(m => m.CustomerId == id);
            //E manda apagalos também
            dc.Meters.DeleteAllOnSubmit(customerMeters);

            dc.Customers.DeleteOnSubmit(customer);

            try
            {
                dc.SubmitChanges();
            }
            catch (Exception e)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.ServiceUnavailable, e));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, "Customer deleted successfully."));

        }
    }
}
