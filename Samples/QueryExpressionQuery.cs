
using WebAPISamplePrototype.QueryExpressionTypes;
using Newtonsoft.Json;
using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading;

namespace WebAPISamplePrototype
{
    public class QueryExpressionQuery
    {
        //Centralized collection of absolute URIs for created entity instances
        private static readonly List<Uri> entityUris = new List<Uri>();

        //Uri for records referenced in this sample
        static Uri account1Uri, contact1Uri;

        public static void Run(CDSWebApiService svc, bool deleteCreatedRecords)
        {

            //  var qe = new QueryExpression("account")
            //  {

            //      ColumnSet = new ColumnSet("name"),
            //      PageInfo = new PagingInfo { Count = 3, PageNumber = 1 },
            //      Criteria = new FilterExpression(LogicalOperator.And)

            //  };
            //  qe.Criteria.AddCondition("name", ConditionOperator.BeginsWith, "Contoso");


            //var results =  svc.Get($"accounts?queryExpression={qe.ToJSON()}");

            /*
            Sample Goals
             - Use ColumnSet to specify attributes
             - Specify whether formatted values should be returned?
             - Get a count of how many records are returned
             
             - Apply Conditions in a query https://docs.microsoft.com/en-us/powerapps/developer/common-data-service/org-service/use-conditionexpression-class
             - Apply multiple conditions in a Filter Expression
             - Page large result sets
             - Page results with a cookie?
             - Use a left outer join in QueryExpression to query for records "not in" https://docs.microsoft.com/en-us/powerapps/developer/common-data-service/org-service/use-left-outer-join-queryexpression-query-records-not-in
             - Show how to use aggregates and grouping
            */

            Console.WriteLine("\n--Starting Query Data with Query Expression--");

            CreateRequiredRecords(svc);

            //Get the id and name of the account created to use as a filter.
            var account1 = svc.Get($"{account1Uri}?$select=accountid,name");
            var account1Id = Guid.Parse(account1["accountid"].ToString());
            string account1Name = (string)account1["name"];

            //Header required to include formatted values
            var formattedValueHeaders = new Dictionary<string, List<string>> {
                { "Prefer", new List<string>
                    { "odata.include-annotations=\"OData.Community.Display.V1.FormattedValue\"" }
                }
            };

            #region Use ColumnSet to specify attributes

            var query1 = new QueryExpression("contact")
            {

                ColumnSet = new ColumnSet("fullname", "jobtitle", "annualincome")

            };
            query1.Criteria.AddCondition("parentcustomerid", ConditionOperator.Equal, account1Id);

            var query1Results = svc.Get(
                $"contacts?queryExpression={query1.ToJSON()}",
                formattedValueHeaders);

            WriteContactResultsTable(
                $"{((JContainer)query1Results["value"]).Count} Contacts related to the account '{account1Name}'",
                query1Results["value"]);

            #endregion Use ColumnSet to specify attributes

            #region Use PagingInfo
            //Re-use the previously defined query
            query1.PageInfo = new PagingInfo() 
            { 
                PageNumber = 1, 
                Count = 4, 
                ReturnTotalRecordCount = true
                //Bug 1686048: Unable to return count when using QueryExpression with Web API
            };

            var top4contactsRelatedToAccount1 = svc.Get(
                $"contacts?queryExpression={query1.ToJSON()}",
                formattedValueHeaders);

            WriteContactResultsTable(
                $"Top 4 Contacts related to the account '{account1Name}'",
                top4contactsRelatedToAccount1["value"]);

            //Get next 4 contacts

            // You can't use the @odata.nextLink property
            // Bug 1686046: Error: 'The query parameter queryExpression is not supported' when using @odata.nextlink returned from Web API QueryExpression response
            //var next4contactsRelatedToAccount1 = svc.Get(
            //    top4contactsRelatedToAccount1["@odata.nextLink"].ToString(), 
            //    formattedValueHeaders);

            //Update the PageNumber to get the next set of result
            query1.PageInfo.PageNumber = 2;

            var next4contactsRelatedToAccount1 = svc.Get(
                $"contacts?queryExpression={query1.ToJSON()}",
                formattedValueHeaders);

            WriteContactResultsTable(
                $"Next 4 Contacts related to the account '{account1Name}'",
                next4contactsRelatedToAccount1["value"]);


            #endregion Use PagingInfo

            #region Apply multiple conditions in a Filter Expression

            var query2 = new QueryExpression("contact")
            {

                ColumnSet = new ColumnSet("fullname", "jobtitle", "annualincome")

            };
            //By default all criteria are evaluated using LogicalOperator.And
            query2.Criteria.AddCondition("parentcustomerid", ConditionOperator.Equal, account1Id);
            query2.Criteria.AddCondition("createdon", ConditionOperator.LastXHours, "1");

            var contactsRelatedToAccount1CreatedInLastHour = svc.Get(
                $"contacts?queryExpression={query2.ToJSON()}",
                formattedValueHeaders);

            WriteContactResultsTable(
                $"Contacts created in the past hour related to '{account1Name}'",
                contactsRelatedToAccount1CreatedInLastHour["value"]);




            var query3 = new QueryExpression("contact")
            {

                ColumnSet = new ColumnSet("fullname", "jobtitle", "annualincome")

            };
            query3.Criteria.AddCondition("parentcustomerid", ConditionOperator.Equal, account1Id);
            query3.Criteria.AddCondition("annualincome", ConditionOperator.GreaterThan, "55000");
            FilterExpression childFilter = query3.Criteria.AddFilter(LogicalOperator.Or);
            childFilter.AddCondition("jobtitle", ConditionOperator.Like, "%senior%");
            childFilter.AddCondition("jobtitle", ConditionOperator.Like, "%manager%");

            var highValueContacts = svc.Get(
                $"contacts?queryExpression={query3.ToJSON()}",
                formattedValueHeaders);

            WriteContactResultsTable(
                $"High salary contacts with 'senior' or 'manager' in job title:",
                highValueContacts["value"]);

            /*
                 var seniorOrSpecialistsCollection = svc.Get("contacts?" +
                "$select=fullname,jobtitle,annualincome&" +
                "$filter=contains(fullname,'(sample)') and " +
                "(contains(jobtitle, 'senior') or " +
                "contains(jobtitle,'specialist')) and annualincome gt 55000", formattedValueHeaders)["value"]; 
            */

            #endregion Apply multiple conditions in a Filter Expression



            DeleteRequiredRecords(svc, deleteCreatedRecords);

            Console.WriteLine("\n--Query Data with Query Expression Completed--");

        }

        private static void CreateRequiredRecords(CDSWebApiService svc)
        {
            var account1 = new JObject {
                { "name", "Contoso, Ltd. (sample)" },
                { "Account_Tasks", new JArray{
                        new JObject{
                                { "subject","Task 1 for Contoso, Ltd."},
                                { "description","Task 1 for Contoso, Ltd. description"}},
                        new JObject{
                                { "subject","Task 2 for Contoso, Ltd."},
                                { "description","Task 2 for Contoso, Ltd. description"}},
                        new JObject{
                                { "subject","Task 3 for Contoso, Ltd."},
                                { "description","Task 3 for Contoso, Ltd. description"}},
                            }
                        },
                { "primarycontactid", new JObject{
                    { "firstname", "Yvonne" },
                    { "lastname", "McKay (sample)" },
                    { "jobtitle", "Coffee Master" },
                    { "annualincome", 45000 },
                    { "Contact_Tasks", new JArray{
                        new JObject{
                                { "subject","Task 1 for Yvonne McKay"},
                                { "description","Task 1 for Yvonne McKay description"}},
                        new JObject{
                                { "subject","Task 2 for Yvonne McKay"},
                                { "description","Task 2 for Yvonne McKay description"}},
                        new JObject{
                                { "subject","Task 3 for Yvonne McKay"},
                                { "description","Task 3 for Yvonne McKay description"}},
                            }
                        }
                    }
                },
                { "contact_customer_accounts", new JArray{
                        new JObject{
                                { "firstname","Susanna"},
                                { "lastname","Stubberod (sample)"},
                                { "jobtitle","Senior Purchaser"},
                                { "annualincome", 52000},
                                { "Contact_Tasks", new JArray{
                                new JObject{
                                        { "subject","Task 1 for Susanna Stubberod"},
                                        { "description","Task 1 for Susanna Stubberod description"}},
                                new JObject{
                                        { "subject","Task 2 for Susanna Stubberod"},
                                        { "description","Task 2 for Susanna Stubberod description"}},
                                new JObject{
                                        { "subject","Task 3 for Susanna Stubberod"},
                                        { "description","Task 3 for Susanna Stubberod description"}},
                                    }
                                }},
                        new JObject{
                                { "firstname","Nancy"},
                                { "lastname","Anderson (sample)"},
                                { "jobtitle","Activities Manager"},
                                { "annualincome", 55500},
                                { "Contact_Tasks", new JArray{
                                new JObject{
                                        { "subject","Task 1 for Nancy Anderson"},
                                        { "description","Task 1 for Nancy Anderson description"}},
                                new JObject{
                                        { "subject","Task 2 for Nancy Anderson"},
                                        { "description","Task 2 for Nancy Anderson description"}},
                                new JObject{
                                        { "subject","Task 3 for Nancy Anderson"},
                                        { "description","Task 3 for Nancy Anderson description"}},
                                    }
                                }},
                        new JObject{
                                { "firstname","Maria"},
                                { "lastname","Cambell (sample)"},
                                { "jobtitle","Accounts Manager"},
                                { "annualincome", 31000},
                                { "Contact_Tasks", new JArray{
                                new JObject{
                                        { "subject","Task 1 for Maria Cambell"},
                                        { "description","Task 1 for Maria Cambell description"}},
                                new JObject{
                                        { "subject","Task 2 for Maria Cambell"},
                                        { "description","Task 2 for Maria Cambell description"}},
                                new JObject{
                                        { "subject","Task 3 for Maria Cambell"},
                                        { "description","Task 3 for Maria Cambell description"}},
                                    }
                                }},
                        new JObject{
                                { "firstname","Scott"},
                                { "lastname","Konersmann (sample)"},
                                { "jobtitle","Accounts Manager"},
                                { "annualincome", 38000},
                                { "Contact_Tasks", new JArray{
                                new JObject{
                                        { "subject","Task 1 for Scott Konersmann"},
                                        { "description","Task 1 for Scott Konersmann description"}},
                                new JObject{
                                        { "subject","Task 2 for Scott Konersmann"},
                                        { "description","Task 2 for Scott Konersmann description"}},
                                new JObject{
                                        { "subject","Task 3 for Scott Konersmann"},
                                        { "description","Task 3 for Scott Konersmann description"}},
                                    }
                                }},
                        new JObject{
                                { "firstname","Robert"},
                                { "lastname","Lyon (sample)"},
                                { "jobtitle","Senior Technician"},
                                { "annualincome", 78000},
                                { "Contact_Tasks", new JArray{
                                new JObject{
                                        { "subject","Task 1 for Robert Lyon"},
                                        { "description","Task 1 for Robert Lyon description"}},
                                new JObject{
                                        { "subject","Task 2 for Robert Lyon"},
                                        { "description","Task 2 for Robert Lyon description"}},
                                new JObject{
                                        { "subject","Task 3 for Robert Lyon"},
                                        { "description","Task 3 for Robert Lyon description"}},
                                    }
                                }},
                        new JObject{
                                { "firstname","Paul"},
                                { "lastname","Cannon (sample)"},
                                { "jobtitle","Ski Instructor"},
                                { "annualincome", 68500},
                                { "Contact_Tasks", new JArray{
                                new JObject{
                                        { "subject","Task 1 for Paul Cannon"},
                                        { "description","Task 1 for Paul Cannon description"}},
                                new JObject{
                                        { "subject","Task 2 for Paul Cannon"},
                                        { "description","Task 2 for Paul Cannon description"}},
                                new JObject{
                                        { "subject","Task 3 for Paul Cannon"},
                                        { "description","Task 3 for Paul Cannon description"}},
                                    }
                                }},
                        new JObject{
                                { "firstname","Rene"},
                                { "lastname","Valdes (sample)"},
                                { "jobtitle","Data Analyst III"},
                                { "annualincome", 86000},
                                { "Contact_Tasks", new JArray{
                                new JObject{
                                        { "subject","Task 1 for Rene Valdes"},
                                        { "description","Task 1 for Rene Valdes description"}},
                                new JObject{
                                        { "subject","Task 2 for Rene Valdes"},
                                        { "description","Task 2 for Rene Valdes description"}},
                                new JObject{
                                        { "subject","Task 3 for Rene Valdes"},
                                        { "description","Task 3 for Rene Valdes description"}},
                                    }
                                }},
                        new JObject{
                                { "firstname","Jim"},
                                { "lastname","Glynn (sample)"},
                                { "jobtitle","Senior International Sales Manager"},
                                { "annualincome", 81400},
                                { "Contact_Tasks", new JArray{
                                new JObject{
                                        { "subject","Task 1 for Jim Glynn"},
                                        { "description","Task 1 for Jim Glynn description"}},
                                new JObject{
                                        { "subject","Task 2 for Jim Glynn"},
                                        { "description","Task 2 for Jim Glynn description"}},
                                new JObject{
                                        { "subject","Task 3 for Jim Glynn"},
                                        { "description","Task 3 for Jim Glynn description"}},
                                    }
                                }}
                            }}
            };



            account1Uri = svc.PostCreate("accounts", account1);
            entityUris.Add(account1Uri);
            contact1Uri = new Uri((svc.Get($"{account1Uri}/primarycontactid/$ref"))["@odata.id"].ToString());
            entityUris.Add(contact1Uri);
            var contact_customer_accounts = svc.Get($"{account1Uri}/contact_customer_accounts/$ref");
            foreach (JObject contactRef in contact_customer_accounts["value"])
            {
                //Add to the top of the list so these are deleted first
                entityUris.Insert(0, new Uri(contactRef["@odata.id"].ToString()));
            }
            //The related tasks will be deleted automatically when the owning record is deleted.
            Console.WriteLine("Account 'Contoso, Ltd. (sample)' created with 1 primary " +
                    "contact and 8 associated contacts.");
        }

        private static void DeleteRequiredRecords(CDSWebApiService svc, bool deleteCreatedRecords)
        {

            if (!deleteCreatedRecords)
            {
                Console.Write("\nDo you want these sample entity records deleted? (y/n) [y]: ");
                string answer = Console.ReadLine();
                answer = answer.Trim();
                if (!(answer.StartsWith("y") || answer.StartsWith("Y") || answer == string.Empty))
                {
                    return;
                }
            }

            Console.WriteLine("\nDeleting data created for this sample:");

            entityUris.ForEach(x =>
            {
                // Console.WriteLine($"\tDeleting {svc.BaseAddress.MakeRelativeUri(x)}"); //remove later
                Console.Write(".");
                svc.Delete(x);
                //Thread.Sleep(100);

            });

            Console.WriteLine("\nData created for this sample deleted.");
        }

        private static void WriteContactResultsTable(string message, JToken collection)
        {
            //Display column widths for contact results table
            const int col1 = -27;
            const int col2 = -35;
            const int col3 = -15;


            Console.WriteLine($"\n{message}");
            //header
            Console.WriteLine($"\t|{"Full Name",col1}|" +
                $"{"Job Title",col2}|" +
                $"{"Annual Income",col3}");
            Console.WriteLine($"\t|{new string('-', col1 * -1),col1}|" +
                $"{new string('-', col2 * -1),col2}|" +
                $"{new string('-', col3 * -1),col3}");
            //rows
            foreach (JObject contact in collection)
            {
                Console.WriteLine($"\t|{contact["fullname"],col1}|" +
                    $"{contact["jobtitle"],col2}|" +
                    $"{contact["annualincome@OData.Community.Display.V1.FormattedValue"],col3}");
            }

        }
    }
}