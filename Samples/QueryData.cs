﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace WebAPISamplePrototype
{
    public class QueryData
    {
        //Centralized collection of absolute URIs for created entity instances
        private static readonly List<Uri> entityUris = new List<Uri>();
        
        //Uri for records referenced in this sample
        static Uri account1Uri, contact1Uri;

        /// <summary>
        /// Runs the sample
        /// </summary>
        /// <param name="svc">Service containing methods to use the Web API.</param>
        /// <param name="deleteCreatedRecords">Whether to delete entities create by this sample.</param>
        public static void Run(CDSWebApiService svc, bool deleteCreatedRecords)
        {
            Console.WriteLine("\n--Starting Query Data --");
            //Create the records that this sample will query.
            CreateRequiredRecords(svc);

            #region Selecting specific properties
            // Basic query: Query using $select against a contact entity to get the properties you want.
            // For performance best practice, always use $select, otherwise all properties are returned
            Console.WriteLine("-- Basic Query --");

            //Header required to include formatted values
            var formattedValueHeaders = new Dictionary<string, List<string>> {
                { "Prefer", new List<string>
                    { "odata.include-annotations=\"OData.Community.Display.V1.FormattedValue\"" }
                }
            };

            var contact1 = svc.Get($"{contact1Uri}?$select=fullname,jobtitle,annualincome", formattedValueHeaders);

            Console.WriteLine($"Contact basic info:\n" +
                $"\tFullname: {contact1["fullname"]}\n" +
                $"\tJobtitle: {contact1["jobtitle"]}\n" +
                $"\tAnnualincome: {contact1["annualincome"]} (unformatted)");

            Console.WriteLine($"\tAnnualincome: {contact1["annualincome@OData.Community.Display.V1.FormattedValue"]} (formatted)\n");

            #endregion Selecting specific properties

            #region Using query functions
            // Filter criteria:
            // Applying filters to get targeted data.
            // 1) Using standard query functions (e.g.: contains, endswith, startswith)
            // 2) Using CDS query functions (e.g.: LastXhours, Last7Days, Today, Between, In, ...)
            // 3) Using filter operators and logical operators (e.g.: eq, ne, gt, and, or, etc…)
            // 4) Set precedence using parenthesis (e.g.: ((criteria1) and (criteria2)) or (criteria3)
            // For more info, see: 
            //https://docs.microsoft.com/en-us/powerapps/developer/common-data-service/webapi/query-data-web-api#filter-results

            Console.WriteLine("-- Filter Criteria --");
            //Filter 1: Using standard query functions to filter results.  In this operation, we 
            //will query for all contacts with fullname containing the string "(sample)".
            var containsSampleinFullNameCollection = svc.Get("contacts?" +
                "$select=fullname,jobtitle,annualincome&" +
                "$filter=contains(fullname,'(sample)')", formattedValueHeaders)["value"];

            WriteContactResultsTable(
                "Contacts filtered by fullname containing '(sample)':",
                containsSampleinFullNameCollection);

            //Filter 2: Using CDS query functions to filter results. In this operation, we will query
            //for all contacts that were created in the last hour. For complete list of CDS query  
            //functions, see: https://docs.microsoft.com/dynamics365/customer-engagement/web-api/queryfunctions

            var createdInLastHourCollection = svc.Get("contacts?" +
            "$select=fullname,jobtitle,annualincome&" +
            "$filter=Microsoft.Dynamics.CRM.LastXHours(PropertyName='createdon',PropertyValue='1')",
            formattedValueHeaders)["value"];

            WriteContactResultsTable("Contacts that were created within the last 1hr:", createdInLastHourCollection);



            //Filter 3: Using operators. Building on the previous operation, we further limit
            //the results by the contact's income. For more info on standard filter operators, 
            //https://docs.microsoft.com/powerapps/developer/common-data-service/webapi/query-data-web-api#filter-results

            var highIncomeContacts = svc.Get("contacts?" +
                "$select=fullname,jobtitle,annualincome&" +
                "$filter=contains(fullname,'(sample)') and annualincome gt 55000", formattedValueHeaders)["value"];

            WriteContactResultsTable("Contacts with (sample) in name and income above $55,000:", highIncomeContacts);


            //Filter 4: Set precedence using parentheses. Continue building on the previous 
            //operation, we further limit results by job title. Parentheses and the order of 
            //filter statements can impact results returned.

            var seniorOrSpecialistsCollection = svc.Get("contacts?" +
                "$select=fullname,jobtitle,annualincome&" +
                "$filter=contains(fullname,'(sample)') and " +
                "(contains(jobtitle, 'senior') or " +
                "contains(jobtitle,'specialist')) and annualincome gt 55000", formattedValueHeaders)["value"];

            WriteContactResultsTable(
                "Contacts with (sample) in name senior jobtitle or high income:",
                seniorOrSpecialistsCollection);

            #endregion Using query functions

            #region Ordering and aliases
            //Results can be ordered in descending or ascending order.
            Console.WriteLine("\n-- Order Results --");

            var orderedResults = svc.Get("contacts?" +
                "$select=fullname,jobtitle,annualincome&" +
                "$filter=contains(fullname,'(sample)')&" +
                "$orderby=jobtitle asc, annualincome desc", formattedValueHeaders)["value"];

            WriteContactResultsTable(
                "Contacts ordered by jobtitle (Ascending) and annualincome (descending)",
                orderedResults);

            //Parameterized aliases can be used as parameters in a query. These parameters can be used 
            //in $filter and $orderby options. Using the previous operation as basis, parameterizing the 
            //query will give us the same results. For more info, see: 
            //https://docs.microsoft.com/powerapps/developer/common-data-service/webapi/use-web-api-functions#passing-parameters-to-a-function

            Console.WriteLine("\n-- Parameterized Aliases --");

            var orderedResultsWithParams = svc.Get("contacts?" +
                "$select=fullname,jobtitle,annualincome&" +
                "$filter=contains(@p1,'(sample)')&" +
                "$orderby=@p2 asc, @p3 desc&" +
                "@p1=fullname&" +
                "@p2=jobtitle&" +
                "@p3=annualincome", formattedValueHeaders)["value"];

            WriteContactResultsTable(
                "Contacts ordered by jobtitle (Ascending) and annualincome (descending)",
                orderedResultsWithParams);


            #endregion Ordering and aliases

            #region Limit results
            //To limit records returned, use the $top query option.  Specifying a limit number for $top 
            //returns at most that number of results per request. Extra results are ignored.
            //For more information, see: 
            // https://docs.microsoft.com/powerapps/developer/common-data-service/webapi/query-data-web-api#use-top-query-option
            Console.WriteLine("\n-- Top Results --");

            var topFive = svc.Get("contacts?" +
                "$select=fullname,jobtitle,annualincome&" +
                "$filter=contains(fullname,'(sample)')&" +
                "$top=5", formattedValueHeaders)["value"];

            WriteContactResultsTable("Contacts top 5 results:", topFive);

            //Result count - count the number of results matching the filter criteria.
            //Tip: Use count together with the "odata.maxpagesize" to calculate the number of pages in
            //the query.  Note: CDS has a max record limit of 5000 records per response.
            Console.WriteLine("\n-- Result Count --");
            //1) Get a count of a collection without the data.
            var count = svc.Get("contacts/$count");
            Console.WriteLine($"\nThe contacts collection has {count} contacts.");
            //  2) Get a count along with the data.

            var countWithData = svc.Get("contacts?" +
                "$select=fullname,jobtitle,annualincome&" +
                "$filter=contains(jobtitle,'senior') or contains(jobtitle, 'manager')" +
                "&$count=true", formattedValueHeaders);

            WriteContactResultsTable($"{countWithData["@odata.count"]} " +
                $"Contacts with 'senior' or 'manager' in job title:",
                countWithData["value"]);


            #endregion Limit results

            #region Expanding results
            //The expand option retrieves related information.  
            //To retrieve information on associated entities in the same request, use the $expand 
            //query option on navigation properties. 
            //  1) Expand using single-valued navigation properties (e.g.: via the 'primarycontactid')
            //  2) Expand using partner property (e.g.: from contact to account via the 'account_primary_contact')
            //  3) Expand using collection-valued navigation properties (e.g.: via the 'contact_customer_accounts')
            //  4) Expand using multiple navigation property types in a single request.
            // Note: Expansions can only go 1 level deep.
            // Tip: For performance best practice, always use $select statement in an expand option.
            Console.WriteLine("\n-- Expanding Results --");

            //1) Expand using the 'primarycontactid' single-valued navigation property of account1.

            var account1 = svc.Get($"{account1Uri}?" +
                $"$select=name&" +
                $"$expand=primarycontactid($select=fullname,jobtitle,annualincome)");

            Console.WriteLine($"Account {account1["name"]} has the following primary contact person:\n" +
             $"\tFullname: {account1["primarycontactid"]["fullname"]} \n" +
             $"\tJobtitle: {account1["primarycontactid"]["jobtitle"]} \n" +
             $"\tAnnualincome: { account1["primarycontactid"]["annualincome"]}");

            //2) Expand using the 'account_primary_contact' partner property.

            var contact2 = svc.Get($"{contact1Uri}?$select=fullname,jobtitle,annualincome&" +
            "$expand=account_primary_contact($select=name)");

            Console.WriteLine($"\nContact '{contact2["fullname"]}' is the primary contact for the following accounts:");
            foreach (JObject account in contact2["account_primary_contact"]) {
                Console.WriteLine($"\t{account["name"]}");
            }

            //3) Expand using the collection-valued 'contact_customer_accounts' navigation property. 

            var account2 = svc.Get($"{account1Uri}?" +
                $"$select=name&" +
                $"$expand=contact_customer_accounts($select=fullname,jobtitle,annualincome)", formattedValueHeaders);

            WriteContactResultsTable(
                $"Account '{account2["name"]}' has the following contact customers:",
                account2["contact_customer_accounts"]);

            //4) Expand using multiple navigation property types in a single request, specifically:
            //   primarycontactid, contact_customer_accounts, and Account_Tasks.

            Console.WriteLine("\n-- Expanding multiple property types in one request -- ");

            var account3 = svc.Get($"{account1Uri}?$select=name&" +
                $"$expand=primarycontactid($select=fullname,jobtitle,annualincome)," +
                $"contact_customer_accounts($select=fullname,jobtitle,annualincome)," +
                $"Account_Tasks($select=subject,description)", formattedValueHeaders);

            Console.WriteLine($"\nAccount {account3["name"]} has the following primary contact person:\n" +
                            $"\tFullname: {account3["primarycontactid"]["fullname"]} \n" +
                            $"\tJobtitle: {account3["primarycontactid"]["jobtitle"]} \n" +
                            $"\tAnnualincome: {account3["primarycontactid"]["annualincome"]}");

            WriteContactResultsTable(
                $"Account '{account3["name"]}' has the following contact customers:",
                account3["contact_customer_accounts"]);

            Console.WriteLine($"\nAccount '{account3["name"] }' has the following tasks:");

            foreach (JObject task in account3["Account_Tasks"])
            {
                Console.WriteLine($"\t{task["subject"]}");
            }

            #endregion Expanding results

            #region FetchXML queries
            //Use FetchXML to query for all contacts whose fullname contains '(sample)'.
            //Note: XML string must be URI encoded. For more information, see: 
            //https://docs.microsoft.com/powerapps/developer/common-data-service/webapi/retrieve-and-execute-predefined-queries#use-custom-fetchxml
            Console.WriteLine("\n-- FetchXML -- ");
            string fetchXmlQuery =
                "<fetch mapping='logical' output-format='xml-platform' version='1.0' distinct='false'>" +
                  "<entity name ='contact'>" +
                    "<attribute name ='fullname' />" +
                    "<attribute name ='jobtitle' />" +
                    "<attribute name ='annualincome' />" +
                    "<order descending ='true' attribute='fullname' />" +
                    "<filter type ='and'>" +
                      "<condition value ='%(sample)%' attribute='fullname' operator='like' />" +
                    "</filter>" +
                  "</entity>" +
                "</fetch>";
            var contacts = svc.Get($"contacts?fetchXml={WebUtility.UrlEncode(fetchXmlQuery)}", formattedValueHeaders);

            WriteContactResultsTable($"Contacts Fetched by fullname containing '(sample)':", contacts["value"]);

            #endregion FetchXML queries

            #region Using predefined queries
            //Use predefined queries of the following two types:
            //  1) Saved query (system view)
            //  2) User query (saved view)
            //For more info, see: 
            //https://docs.microsoft.com/powerapps/developer/common-data-service/webapi/retrieve-and-execute-predefined-queries#predefined-queries

            //1) Saved Query - retrieve "Active Accounts", run it, then display the results.
            Console.WriteLine("\n-- Saved Query -- ");

            var savedqueryid = svc.Get("savedqueries?" +
                "$select=name,savedqueryid&" +
                "$filter=name eq 'Active Accounts'")["value"][0]["savedqueryid"];

            var activeAccounts = svc.Get($"accounts?savedQuery={savedqueryid}", formattedValueHeaders)["value"] as JArray;

            DisplayFormattedEntities("Active Accounts", activeAccounts, new string[] { "name" });

            //2) Create a user query, then retrieve and execute it to display its results.
            //For more info, see: 
            //https://docs.microsoft.com/powerapps/developer/common-data-service/saved-queries
            Console.WriteLine("\n-- User Query -- ");
            var userQuery = new JObject
            {
                ["name"] = "My User Query",
                ["description"] = "User query to display contact info.",
                ["querytype"] = 0,
                ["returnedtypecode"] = "contact",
                ["fetchxml"] = @"<fetch mapping='logical' output-format='xml-platform' version='1.0' distinct='false'>
                    <entity name ='contact'>
                        <attribute name ='fullname' />
                        <attribute name ='contactid' />
                        <attribute name ='jobtitle' />
                        <attribute name ='annualincome' />
                        <order descending ='false' attribute='fullname' />
                        <filter type ='and'>
                            <condition value ='%(sample)%' attribute='fullname' operator='like' />
                            <condition value ='%Manager%' attribute='jobtitle' operator='like' />
                            <condition value ='55000' attribute='annualincome' operator='gt' />
                        </filter>
                    </entity>
                 </fetch>"
            };

            //Create the saved query
            var myUserQueryUri = svc.PostCreate("userqueries", userQuery);
            entityUris.Add(myUserQueryUri); //To delete later
            //Retrieve the userqueryid
            var myUserQueryId = svc.Get($"{myUserQueryUri}/userqueryid")["value"];
            //Use the query to return results:
            var myUserQueryResults = svc.Get($"contacts?userQuery={myUserQueryId}",formattedValueHeaders)["value"];

            WriteContactResultsTable($"Contacts Fetched by My User Query:", myUserQueryResults);

            #endregion Using predefined queries

            DeleteRequiredRecords(svc, deleteCreatedRecords);

            Console.WriteLine("\n--Query Data Completed--");
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
            List<Uri> tryAgainEntities = new List<Uri>();

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

            Console.WriteLine("\nDeleting data created for this sample...");

            entityUris.ForEach(x =>
            {
                Thread.Sleep(100); //Slow things down a bit
                try
                {
                    svc.Delete(x);
                }
                catch (Exception)
                {

                    tryAgainEntities.Add(x);
                }

            });

            tryAgainEntities.ForEach(x =>
            {
                try
                {
                    svc.Delete(x);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not delete entity at {x}");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Unexpected Error: {ex.Message}");
                    Console.ResetColor();
                }

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

        /// <summary> Displays formatted entity collections to the console. </summary>
        /// <param name="label">Descriptive text output before collection contents </param>
        /// <param name="collection"> JObject containing array of entities to output by property </param>
        /// <param name="properties"> Array of properties within each entity to output. </param>
        private static void DisplayFormattedEntities(string label, JArray entities, string[] properties)
        {
            Console.Write(label);
            int lineNum = 0;
            foreach (JObject entity in entities)
            {
                lineNum++;
                List<string> propsOutput = new List<string>();
                //Iterate through each requested property and output either formatted value if one 
                //exists, otherwise output plain value.
                foreach (string prop in properties)
                {
                    string propValue;
                    string formattedProp = prop + "@OData.Community.Display.V1.FormattedValue";
                    if (null != entity[formattedProp])
                    {
                        propValue = entity[formattedProp].ToString();
                    }
                    else
                    {
                        if (null != entity[prop])
                        {
                            propValue = entity[prop].ToString();
                        }
                        else
                        {

                            propValue = "NULL";
                        }

                    }
                    propsOutput.Add(propValue);
                }
                Console.Write("\n\t{0}) {1}", lineNum, string.Join(", ", propsOutput));
            }
            Console.Write("\n");
        }
    }
}
