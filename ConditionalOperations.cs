using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace WebAPISamplePrototype
{
    public class ConditionalOperations
    {

        public static void Run(CDSWebApiService svc)
        {
            /// <summary> Creates the CRM entity instance used by this sample. </summary>
            #region Create sample Account record
            // Create a CRM account record.
            Console.WriteLine("\nCreate sample data");
            var account = new JObject
            {
                { "name", "Contoso Ltd" },
                { "telephone1", "555-0000" }, //Phone number value will increment with each update attempt
                { "revenue", 5000000 },
                { "description", "Parent company of Contoso Pharmaceuticals, etc." }
            };

            var accountUri = svc.PostCreate("accounts", account);
            Console.WriteLine("Account entity created:");

            //Retrieve the account record you created to access the ETag value
            account = svc.Get($"{accountUri}?$select=name,revenue,telephone1,description") as JObject;
            var initialAcctETagVal = account["@odata.etag"].ToString();
            var updatedAcctETagVal = string.Empty;
            Console.WriteLine($"ETag value: {initialAcctETagVal}");
            #endregion Create sample Account record

            #region Conditional GET 
            Console.WriteLine("\n--Conditional GET section started--");
            var IfNoneMatchHeader = new Dictionary<string, List<string>>
            {
                { "If-None-Match", new List<string> { initialAcctETagVal } }
            };
            // Retrieve only if it doesn't match previously retrieved version.
            var result = svc.Get($"{accountUri}?$select=name,revenue,telephone1,description", IfNoneMatchHeader);
            if (result == null)
            {
                Console.WriteLine("Expected outcome: Entity was not modified so nothing was returned.");
            }
            else
            {
                Console.WriteLine("Unexpected outcome: Entity was not modified so nothing should be returned.");
            }

            // Modify the account instance by updating telephone1
            svc.Put(accountUri, "telephone1", "555-0001");
            Console.WriteLine("\nAccount telephone number updated.");

            // Re-Attempt conditional GET with original initialAcctETagVal ETag value
            result = svc.Get($"{accountUri}?$select=name", IfNoneMatchHeader);
            if (result == null)
            {
                Console.WriteLine("Unexpected outcome: Entity was modified so something should be returned.");

            }
            else
            {
                Console.WriteLine("Expected outcome: Entity was modified so something was returned.");
            }

            #endregion Conditional GET  

            #region Optimistic concurrency on delete and update
            Console.WriteLine("\n--Optimistic concurrency section started--");
            // Attempt to delete original account (if matches original initialAcctETagVal ETag value).

            var IfMatchHeader = new Dictionary<string, List<string>>
            {
                { "If-Match", new List<string> { initialAcctETagVal } }
            };
            try
            {
                svc.Delete(accountUri, IfMatchHeader);
            }
            catch (CDSWebApiException ex)
            {
                Console.WriteLine($"Expected Error: {ex.Message}\n" +
                    $"\tAccount not deleted using the initial ETag value: {initialAcctETagVal}\n" +
                    $"\tStatusCode: {ex.StatusCode}\n" +
                    $"\tReasonPhrase: {ex.ReasonPhrase}");
            }

            //Attempt to update account (if matches original ETag value).


            JObject accountUpdate = new JObject
            {
                { "telephone1", "555-0002" },
                { "revenue", 6000000 }
            };
            try
            {
                svc.Patch(accountUri, accountUpdate, IfMatchHeader);
            }
            catch (CDSWebApiException ex)
            {
                Console.WriteLine($"Expected Error: {ex.Message}\n" +
                $"\tAccount not updated using the initial ETag value: {initialAcctETagVal}\n" +
                $"\tStatusCode: {ex.StatusCode}\n" +
                $"\tReasonPhrase: {ex.ReasonPhrase}");
            }

            //Get current ETag value:
            updatedAcctETagVal = svc.Get($"{accountUri}?$select=accountid")["@odata.etag"].ToString();

            // Reattempt update if matches current ETag value.
            var NewIfMatchHeader = new Dictionary<string, List<string>>
            {
                { "If-Match", new List<string> { updatedAcctETagVal } }
            };

            svc.Patch(accountUri, accountUpdate, NewIfMatchHeader);
            Console.WriteLine($"\nAccount successfully updated using ETag: {updatedAcctETagVal}");

            // Retrieve and output current account state.
            account = svc.Get($"{accountUri}?$select=name,revenue,telephone1,description") as JObject;
            Console.WriteLine(account.ToString(Formatting.Indented));

            // Delete the account record
            svc.Delete(accountUri);
            Console.WriteLine("Account Deleted.");
            #endregion Optimistic concurrency on delete and update

            #region Controlling upsert operations
            Console.WriteLine("\n--Controlling upsert operations section started--");
            // Attempt to update it only if it exists
            accountUpdate["telephone1"] = "555-0006";
            var IfMatchAnyHeader = new Dictionary<string, List<string>>
            {
                { "If-Match", new List<string> {"*"} }
            };

            try
            {
                svc.Patch(accountUri, accountUpdate, IfMatchAnyHeader);
            }
            catch (CDSWebApiException ex)
            {
                Console.WriteLine($"Expected Error: {ex.Message}\n" +
                $"\tAccount not updated because it does not exist.\n" +
                $"\tStatusCode: {ex.StatusCode}\n" +
                $"\tReasonPhrase: {ex.ReasonPhrase}");
            }
            //Attempt to upsert to re-create the record that was deleted 
            // as long as there are no existing account records with the same id.
            var IfNoneMatchAnyHeader = new Dictionary<string, List<string>>
            {
                { "If-None-Match", new List<string> {"*"} }
            };

            //Remove any lookup properties since they cannot be set.
            account.Remove("_transactioncurrencyid_value");

            svc.Patch(accountUri, account, IfNoneMatchAnyHeader);

            Console.WriteLine("Account upserted.");

            #endregion Controlling upsert operations

            #region Clean-up
            //Delete the account record for good
            svc.Delete(accountUri);
            #endregion Clean-up

        }
    }
}
