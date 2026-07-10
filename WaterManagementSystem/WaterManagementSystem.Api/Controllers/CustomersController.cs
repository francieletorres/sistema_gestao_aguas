using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Linq;
using System.Net.Http;
using System.Net;

namespace WaterManagementSystem.Api.Controllers
{
    public class CustomersController : ApiController
    {
        WaterManagementDataContext dc = new WaterManagementDataContext(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=WaterManagementSystemDB;Integrated Security=True");

        // GET: api/Customers
        public List<Customer> Get()
        {
            var list = from customer
                        in dc.Customers
                       select customer;

            return list.ToList();

        }

        // GET: api/Customers/5
        public IHttpActionResult Get(int id)
        {
            var customer = dc.Customers.SingleOrDefault(c => c.CustomerId == id);

            if (customer != null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, customer));
            }

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "Customer not found"));
        }

        // POST: api/Customers
        public IHttpActionResult Post([FromBody] Customer newCustomer)
        {
            if (newCustomer == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid customer data."));
            }


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
        public IHttpActionResult Put(int id, [FromBody] Customer updateCustomer)
        {
            if (updateCustomer == null)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid customer data."));
            }

            Customer customer = dc.Customers.FirstOrDefault(c => c.CustomerId == id);

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
        public IHttpActionResult Delete(int id)
        {
            Customer customer = dc.Customers.FirstOrDefault(c => c.CustomerId == id);

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
