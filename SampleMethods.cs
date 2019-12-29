using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace WebAPISamplePrototype
{
    public partial class SampleProgram
    {
        public static string GetParameterValueFromConnectionString(
            string connectionString,
            string parameter)
        {
            try
            {
                string value = connectionString
                    .Split(';')
                    .Where(s => s.Trim()
                    .StartsWith(parameter)).
                    FirstOrDefault()
                    .Split('=')[1];
                if (value.ToLower() == "null")
                {
                    return string.Empty;
                }
                return value;
            }
            catch (Exception)
            {
                return string.Empty;
            }

        }
    }

    public partial class FunctionsAndActions
    {
        //Centralized collection of entity URIs used to manage lifetimes
        private static readonly List<Uri> entityUris = new List<Uri>();
        private static Uri incident1Uri, opportunity1Uri, contact1Uri;
        //Associated solution package that contains custom actions used by this sample
        const string customSolutionFilename = "WebAPIFunctionsandActions_1_0_0_0_managed.zip";
        const string customSolutionName = "WebAPIFunctionsandActions";
        static string customSolutionID = null;

        private static void InstallFunctionsAndActionsSolution(CDSWebApiService svc)
        {
            if (!IsSolutionInstalled(svc))
            {
                //Install the solution
                //Locate the custom solution zip file, which should have been copied over to the build 
                //output directory.
                string solutionPath = Directory.GetCurrentDirectory() + "\\" + customSolutionFilename;
                if (!File.Exists(solutionPath))
                { return; }
                //Read the solution package into memory
                byte[] packageBytes = File.ReadAllBytes(solutionPath);

                Guid ImportJobId = Guid.NewGuid();
                //Import the solution package.
                JObject importParams = new JObject
                {
                    ["CustomizationFile"] = packageBytes,
                    ["OverwriteUnmanagedCustomizations"] = false,
                    ["PublishWorkflows"] = false,
                    ["ImportJobId"] = ImportJobId
                };

                svc.Post("ImportSolution", importParams);

                if (IsSolutionInstalled(svc))
                {
                    Console.WriteLine("Solution containing custom actions was installed.");
                }
            }
            else
            {
                Console.WriteLine("Solution containing custom actions was already installed.");
            }

        }

        private static bool IsSolutionInstalled(CDSWebApiService svc)
        {
            //Check whether solution is already installed
            JObject solutionsResult = svc.Get("solutions?" +
                "$select=solutionid&" +
                $"$filter=uniquename eq '{customSolutionName}'") as JObject;

            var IsInstalled = solutionsResult["value"].Any();
            if (IsInstalled)
            {
                customSolutionID = solutionsResult["value"][0]["solutionid"].ToString();
            }

            return IsInstalled;
        }

        private static void UnInstallFunctionsAndActionsSolution(CDSWebApiService svc)
        {
            if (IsSolutionInstalled(svc))
            {

                svc.Delete(new Uri($"{svc.BaseAddress}solutions({customSolutionID})"));
                Console.WriteLine("Solution containing custom actions was uninstalled.");
            }

        }

        private static void CreateRequiredRecords(CDSWebApiService svc)
        {
            //Create a parent account, an associated incident with three associated tasks 
            //(required for CalculateTotalTimeIncident).
            Console.WriteLine("----Creating Required Records---- -");

            JObject account1 = new JObject(new JProperty("name", "Fourth Coffee"));
            var account1Uri = svc.PostCreate("accounts", account1);
            entityUris.Add(account1Uri);
            JObject incident1 = new JObject
            (
             new JProperty("title", "Sample Case"),
             new JProperty("customerid_account@odata.bind", account1Uri),
                new JProperty("Incident_Tasks",
                    new JArray(
                        new JObject(
                            new JProperty("subject", "Task 1"),
                            new JProperty("actualdurationminutes", 30)
                                ),
                        new JObject(
                            new JProperty("subject", "Task 2"),
                            new JProperty("actualdurationminutes", 30)
                                ),
                        new JObject(
                            new JProperty("subject", "Task 3"),
                            new JProperty("actualdurationminutes", 30)
            ))));

            incident1Uri = svc.PostCreate("incidents", incident1);

            Console.WriteLine("Incident and related tasks are Created");

            //Close the associated tasks (so that they represent completed work).
            JObject completeCode = new JObject(
                new JProperty("statecode", 1),
                new JProperty("statuscode", 5));

            var incidentTaskRefs = svc.Get($"{incident1Uri}/Incident_Tasks/$ref");

            foreach (JObject obj in incidentTaskRefs["value"])
            {
                svc.Patch(new Uri(obj["@odata.id"].ToString()), completeCode);
            }
            Console.WriteLine("Tasks are closed.");

            //Create another account and associated opportunity (required for CloseOpportunityAsWon).
            JObject account2 = new JObject(
                new JProperty("name", "Coho Winery"),
                new JProperty("opportunity_customer_accounts",
                new JArray(
                    new JObject(
                        new JProperty("name", "Opportunity to win")
            ))));
            var account2Uri = svc.PostCreate("accounts", account2);
            entityUris.Add(account2Uri);
            Console.WriteLine("Another Account is created with associated Opportunity");
            //Retrieve the URI to the opportunity.
            var custOpporRefs = svc.Get($"{account2Uri}/opportunity_customer_accounts/$ref");
            opportunity1Uri = new Uri(custOpporRefs["value"][0]["@odata.id"].ToString());

            //Create a contact to use with custom action sample_AddNoteToContact 
            var contact1 = new JObject(
                new JProperty("firstname", "Jon"),
                new JProperty("lastname", "Fogg"));
            contact1Uri = svc.PostCreate("contacts", contact1);
            Console.WriteLine("Contact record is created");
            entityUris.Add(contact1Uri);

        }

        private static void DeleteRequiredRecords(CDSWebApiService svc)
        {
            entityUris.ForEach(x =>
            {
                svc.Delete(x);
            });
        }
    }

    public partial class QueryData
    {
        //Display column widths for contact results table
        const int col1 = -27;
        const int col2 = -35;
        const int col3 = -15;

        //Centralized collection of absolute URIs for created entity instances
        private static readonly List<Uri> entityUris = new List<Uri>();
        private static readonly bool prompt = true;

        static Uri account1Uri, contact1Uri;

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

