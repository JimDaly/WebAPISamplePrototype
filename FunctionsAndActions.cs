using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace WebAPISamplePrototype
{
    public partial class FunctionsAndActions
    {
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
            var queueResponse = svc.Post($"{myQueueUri}/Microsoft.Dynamics.CRM.AddToQueue",addToQueueParams);
            var queueItemId = queueResponse["QueueItemId"].ToString();
            Console.WriteLine($"\tQueueItemId returned from AddToQueue action: {queueItemId}");

            #endregion Call a bound action that requires parameters.

            InstallFunctionsAndActionsSolution(svc);
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
            var customerParam = new JObject { 
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

            //Delete records used by this sample
            Console.WriteLine("Deleting records created for this sample.");
            DeleteRequiredRecords(svc);

            Console.WriteLine("\n--Functions And Actions Completed--");
        }
    }
}
