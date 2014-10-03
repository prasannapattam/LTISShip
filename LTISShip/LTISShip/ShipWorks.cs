using Act.Framework;
using Act.Framework.Contacts;
using Act.Framework.Lookups;
using Act.Framework.Opportunities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTISShip
{
    public class ShipWorks
    {
        public long GetLastOrderNumber()
        {
            string cmdText = "select Value from Configuration where Name = 'OrderNumber'";
            return Convert.ToInt64(DBUtil.ExecuteScalar(ConnectionStrings.LTIS, cmdText));
        }

        public void UpdateOrderNumber(long orderNumber)
        {
            string cmdText = "update Configuration set Value = '" + orderNumber.ToString() + "' where Name = 'OrderNumber'";
            DBUtil.Execute(ConnectionStrings.LTIS, cmdText);
        }

        public void AuditRun(int orderCount)
        {
            string cmdText = "insert into ShipWorksRun(OrderCount) values(" + orderCount.ToString() + ")";
            DBUtil.Execute(ConnectionStrings.LTIS, cmdText);
        }

        public DataTable GetOrders(long orderNumber)
        {
            string cmdText = @"select o.OrderID, o.OrderNumber, o.BillFirstName, o.BillLastName, o.BillCompany, o.BillEmail,
                                StreetAddress = IsNull(o.BillStreet1, '') + ' ' + IsNull(o.BillStreet2, '') + ' ' + IsNull(o.BillStreet3, ''),
                                o.BillCity, o.BillStateProvCode, o.BillPostalCode, o.BillPhone,
                                o.RollupItemCount, oi.Name, oi.UnitPrice, oi.Quantity 
                                from [Order] as o
                                inner join OrderItem as oi on o.OrderID = oi.OrderID
                                and OrderNumber > " + orderNumber.ToString() + @"
                                order by o.OrderNumber";

            return DBUtil.ExecuteDataTable(ConnectionStrings.ShipWorks, cmdText);
        }

        public void Process()
        {
            long orderNumber = GetLastOrderNumber();

            DataTable orders = GetOrders(orderNumber);
            AuditRun(orders.Rows.Count);
            if(orders.Rows.Count == 0)
                return;

            List<OpportunityModel> opportunities = ConvertToModel(orders);

            long currentOrderNumber = orderNumber;

            using (ActConnection act = new ActConnection())
            {
                foreach (var opportunity in opportunities)
                {
                    currentOrderNumber = opportunity.OrderNumber;

                    var actOpportunity = CreateOpportunity(act.Framework, opportunity);
                    foreach (var product in opportunity.products)
                    {
                        AddProducts(act.Framework, actOpportunity, product);
                    }

                    AddCustomer(act.Framework, actOpportunity, opportunity.Contact);

                }
            }

            if (currentOrderNumber != orderNumber)
            {
                UpdateOrderNumber(currentOrderNumber);
            }
        }

        private List<OpportunityModel> ConvertToModel(DataTable orders)
        {
            List<OpportunityModel> opportunities = new List<OpportunityModel>();
            OpportunityModel opportunity;
            for(int rowCount = 0; rowCount < orders.Rows.Count; rowCount ++)
            {
                DataRow order = orders.Rows[rowCount];
                opportunity = new OpportunityModel();

                opportunity.OrderNumber = Convert.ToInt64(order["OrderNumber"]);
                opportunity.Name = "Order # " + order["OrderNumber"].ToString();
                opportunity.Contact = new ContactModel();
                opportunity.Contact.FirstName = DBUtil.DBString(order["BillFirstName"]);
                opportunity.Contact.LastName = DBUtil.DBString(order["BillLastName"]);
                opportunity.Contact.Organization = DBUtil.DBString(order["BillCompany"]);
                opportunity.Contact.EmailAddress = DBUtil.DBString(order["BillEmail"]);
                opportunity.Contact.StreetAddress = DBUtil.DBString(order["StreetAddress"]).Trim();
                opportunity.Contact.City = DBUtil.DBString(order["BillCity"]);
                opportunity.Contact.State = DBUtil.DBString(order["BillStateProvCode"]);
                opportunity.Contact.Zip = DBUtil.DBString(order["BillPostalCode"]);
                opportunity.Contact.Phone = DBUtil.DBString(order["BillPhone"]);

                //getting the products 
                int productCount = Convert.ToInt32(order["RollupItemCount"]);
                opportunity.products = new List<OpportuityProductModel>();
                for (var count = 0; count < productCount; count ++ )
                {
                    if (count != 0)
                    {
                        rowCount++;
                        order = orders.Rows[rowCount];
                    }
                    var product = new OpportuityProductModel();
                    product.Name = DBUtil.DBString(order["Name"]);
                    product.Quantity = Convert.ToDecimal(order["Quantity"]);
                    product.Cost = Convert.ToDecimal(order["UnitPrice"]);
                    product.Price = Convert.ToDecimal(order["UnitPrice"]);
                    product.Discount = 0;
                    opportunity.products.Add(product);
                }
                opportunities.Add(opportunity);
            }

            return opportunities;
        }

        private Opportunity CreateOpportunity(ActFramework act, OpportunityModel model)
        {
            Opportunity opportunity = act.Opportunities.CreateOpportunity();
            opportunity.Name = model.Name;
            opportunity.Update();
            
            return opportunity;
        }

        private void AddProducts(ActFramework act, Opportunity opportunity, OpportuityProductModel model)
        {
            OpportunityProduct product = act.Products.OpportunityProductManager.CreateCustomEntity();
            product.Name = model.Name;
            product.Quantity = model.Quantity;
            product.Cost = model.Cost;
            product.Price = model.Price;
            product.Discount = model.Discount;
            product.SetOpportunities(act.Opportunities.GetOpportunityAsOpportunityList(opportunity));
            product.Update();
        }

        private void AddCustomer(ActFramework act, Opportunity opportunity, ContactModel model)
        {
            Contact actContact = null;
            ContactList list = GetContactsFromEmail(model.EmailAddress, act);
            if (list.Count > 0)
                actContact = list[0];
            else
                actContact = act.Contacts.CreateContact();

            SetContactAttributes(model, actContact);

            opportunity.UpdateContacts(new Guid[] { actContact.ID }, null);

        }

        private ContactList GetContactsFromEmail(string email, ActFramework act)
        {
            CriteriaColumn actColumn = act.Lookups.GetCriteriaColumn("TBL_CONTACT", "BUSINESS_EMAIL", true);
            Criteria[] actCriteria = {
                    new Criteria(LogicalOperator.End, 0, 0, actColumn, OperatorEnum.EqualTo, email)
                };

            ContactLookup actLookup = act.Lookups.LookupContactsReplace(actCriteria, true, true);
            ContactList actContacts = actLookup.GetContacts(null);

            return actContacts;
        }

        private void SetContactAttributes(ContactModel model, Contact actContact)
        {
            actContact.FullName = model.FirstName + " " + model.LastName;
            actContact.Company = model.Organization;
            actContact.Fields["Contact.E-mail", false] = model.EmailAddress;
            actContact.Fields["Contact.Address 1", false] = model.StreetAddress;
            //actContact.Fields["Contact.Address 2", false] = model.Address2;
            actContact.Fields["Contact.City", false] = model.City;
            actContact.Fields["Contact.State", false] = model.State;
            actContact.Fields["Contact.ZIP Code", false] = model.Zip;
            actContact.Fields["Contact.Phone", false] = model.Phone;
            actContact.Update();
        }



    }
}
