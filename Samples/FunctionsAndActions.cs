using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WebAPISamplePrototype
{
    public class FunctionsAndActions
    {
        //Centralized collection of entity URIs used to manage lifetimes
        private static readonly List<Uri> entityUris = new List<Uri>();
        private static Uri incident1Uri, opportunity1Uri, contact1Uri;
        //Associated solution package that contains custom actions used by this sample
        const string customSolutionFilename = "WebAPIFunctionsandActions_1_0_0_0_managed.zip";
        const string customSolutionName = "WebAPIFunctionsandActions";
        static string customSolutionID = null;

        public static void Run(CDSWebApiService svc)
        {
            Console.WriteLine("\n--Starting Functions And Actions--");

            //Create records required for this sample
            CreateRequiredRecords(svc);

            #region Call an unbound function with no parameters.
            //Retrieve the current user's full name from the WhoAmI function:
            // https://docs.microsoft.com/dynamics365/customer-engagement/web-api/whoami
            // Which returns a WhoAmIResponse ComplexType 
            // https://docs.microsoft.com/dynamics365/customer-engagement/web-api/whoamiresponse
            // Which contains a UserId property that can be used to retrieve information about the current user
            Console.WriteLine("Unbound function: WhoAmI");
            var whoAmIresp = svc.Get("WhoAmI");
            //Then retrieve the full name for that unique ID.
            var myUserId = whoAmIresp["UserId"];
            var user = svc.Get($"systemusers({myUserId})?$select=fullname");
            Console.WriteLine($"\tCurrent user has full name: '{user["fullname"]}'.");
            #endregion Call an unbound function with no parameters.

            #region Call an unbound function that requires parameters.
            //Retrieve the time zone code for the specified time zone, using the GetTimeZoneCodeByLocalizedName 
            //function: https://docs.microsoft.com/dynamics365/customer-engagement/web-api/gettimezonecodebylocalizedname, 
            // which returns a GetTimeZoneCodeByLocalizedNameResponse ComplexType 
            // https://docs.microsoft.com/dynamics365/customer-engagement/web-api/gettimezonecodebylocalizednameresponse
            string timeZoneName = "Pacific Standard Time";
            int localeID = 1033;
            Console.WriteLine("Unbound function with parameters: GetTimeZoneCodeByLocalizedName");
            var LocalizedNameResponse = svc.Get($"GetTimeZoneCodeByLocalizedName" +
                $"(LocalizedStandardName=@p1,LocaleId=@p2)" +
                $"?@p1='{timeZoneName}'&@p2={localeID}");

            Console.WriteLine($"\tThe time zone '{timeZoneName}' has the code '{LocalizedNameResponse["TimeZoneCode"]}'.");
            #endregion Call an unbound function that requires parameters.

            #region Call a bound function.
            //Retrieve the total time, in minutes, spent on all tasks associated with an incident.
            //Uses the CalculateTotalTimeIncident function: 
            //https://docs.microsoft.com/dynamics365/customer-engagement/web-api/calculatetotaltimeincident, 
            //which returns a CalculateTotalTimeIncidentResponse complex type: 
            //https://docs.microsoft.com/en-us/dynamics365/customer-engagement/web-api/calculatetotaltimeincidentresponse.

            Console.WriteLine("Bound function: CalculateTotalTimeIncident");
            var cttir = svc.Get($"{incident1Uri}/Microsoft.Dynamics.CRM.CalculateTotalTimeIncident()");
            Console.WriteLine($"\tThe total duration of tasks associated with the " +
                $"incident is {cttir["TotalTime"]} minutes.");
            #endregion Call a bound function.

            #region Call an unbound action that requires parameters.
            //Close an opportunity and mark it as won. Uses the WinOpportunity action: 
            //https://docs.microsoft.com/en-us/dynamics365/customer-engagement/web-api/winopportunity, 
            //which takes a int32 status code and an opportunityclose entity type: 
            //https://docs.microsoft.com/en-us/dynamics365/customer-engagement/web-api/opportunityclose.

            Console.WriteLine("UnBound function: WinOpportunity");
            var winOpportParams = new JObject(
                new JProperty("Status", 3),
                new JProperty("OpportunityClose", new JObject(
                    new JProperty("subject", "Won Opportunity"),
                    new JProperty("opportunityid@odata.bind", opportunity1Uri)
                )));

            svc.Post("WinOpportunity", winOpportParams);
            Console.WriteLine("\tOpportunity won.");

            #endregion Call an unbound action that requires parameters.

            #region Call a bound action that requires parameters.
            //Add a new letter tracking activity to the current user's queue. Uses the AddToQueue 
            //action: https://docs.microsoft.com/en-us/dynamics365/customer-engagement/web-api/addtoqueue, which is bound to the queue 
            //entity type: https://docs.microsoft.com/en-us/powerapps/developer/common-data-service/reference/entities/queue, and returns a 
            //AddToQueueResponse complex type: https://docs.microsoft.com/en-us/dynamics365/customer-engagement/web-api/addtoqueueresponse.
            JObject letter = new JObject
            {
                ["description"] = "Example letter"
            };
            var letterUri = svc.PostCreate("letters", letter);
            var letterActivityId = (svc.Get($"{letterUri}/activityid"))["value"];

            var queueRef = svc.Get($"systemusers({myUserId})/queueid/$ref");
            var myQueueUri = new Uri(queueRef["@odata.id"].ToString());

            JObject addToQueueParams = new JObject(
                new JProperty("Target",
                    new JObject(
                        new JProperty("activityid", letterActivityId),
                        new JProperty("@odata.type", "Microsoft.Dynamics.CRM.letter")
                )));

            Console.WriteLine("Bound action: AddToQueue");
            var queueResponse = svc.Post($"{myQueueUri}/Microsoft.Dynamics.CRM.AddToQueue", addToQueueParams);
            var queueItemId = queueResponse["QueueItemId"].ToString();
            Console.WriteLine($"\tQueueItemId returned from AddToQueue action: {queueItemId}");

            #endregion Call a bound action that requires parameters.

            if (InstallFunctionsAndActionsSolution(svc))
            {
                #region Call a bound custom action that requires parameters.

                //Add a note to a specified contact. Uses the custom action sample_AddNoteToContact, which
                //is bound to the contact to annotate, and takes a single param, the note to add. It also  
                //returns the URI to the new annotation. 
                Console.WriteLine("Custom action: sample_AddNoteToContact");
                var note1 = new JObject
                {
                    ["NoteTitle"] = "Note Title",
                    ["NoteText"] = "The text content of the note."
                };
                var noteRef = svc.Post($"{contact1Uri}/Microsoft.Dynamics.CRM.sample_AddNoteToContact", note1);

                //Retrieve the created note and the related contact full name;
                var noteAndContact = svc.Get($"{svc.BaseAddress}annotations({noteRef["annotationid"]})" +
                    $"?$select=subject,notetext&$expand=objectid_contact($select=fullname)");

                Console.WriteLine($"\tA note with the subject '{noteAndContact["subject"]}' \n" +
                    $"\tand the text '{noteAndContact["notetext"]}' was created and associated with \n" +
                    $"\tthe contact '{noteAndContact["objectid_contact"]["fullname"]}'");

                #endregion Call a bound custom action that requires parameters.

                #region Call an unbound custom action that requires parameters.
                //Create a customer of the specified type, using the custom action sample_CreateCustomer,
                //which takes two parameters: the type of customer ('account' or 'contact') and the name of 
                //the new customer.
                var customerParam = new JObject
                {
                    ["CustomerType"] = "account",
                    ["AccountName"] = "New account customer (sample)"
                };
                Console.WriteLine("Custom action: sample_CreateCustomer");
                svc.Post("sample_CreateCustomer", customerParam);
                Console.WriteLine($"\tThe account '{customerParam["AccountName"]}' was created.");

                //Try to call the same custom action with invalid parameters, here the same name is
                //not valid for a contact. (ContactFirstname and ContactLastName parameters are  
                //required when CustomerType is contact.)
                var invalidCustomerParam = new JObject
                {
                    ["CustomerType"] = "contact",
                    ["AccountName"] = "New account customer (sample)"
                };
                try
                {
                    Console.WriteLine("Custom action: sample_CreateCustomer with invalid parameters:");
                    svc.Post("sample_CreateCustomer", invalidCustomerParam);
                }
                catch (CDSWebApiException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Expected Error:\t{ex.Message}\n" +
                    $"\tStatusCode: {ex.StatusCode}\n" +
                    $"\tReasonPhrase: {ex.ReasonPhrase}\n" +
                    $"\tErrorCode: {ex.ErrorCode}");
                    Console.ResetColor();
                }

                #endregion Call an unbound custom action that requires parameters.
                Console.WriteLine("Uninstalling solution containing custom actions.");
                UnInstallFunctionsAndActionsSolution(svc);
            }
            //Delete records used by this sample
            Console.WriteLine("Deleting records created for this sample.");
            DeleteRequiredRecords(svc);

            Console.WriteLine("\n--Functions And Actions Completed--");
        }

        private static bool InstallFunctionsAndActionsSolution(CDSWebApiService svc)
        {
            if (!IsSolutionInstalled(svc))
            {
                //Install the solution
                //Locate the custom solution zip file, which should have been copied over to the build 
                //output directory.
                string solutionPath = Directory.GetCurrentDirectory() + "\\SolutionFiles\\" + customSolutionFilename;
                if (!File.Exists(solutionPath))
                {
                    Console.WriteLine($"Solution file not found at {solutionPath}.");
                    return false;
                }
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
            return true;

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
}
