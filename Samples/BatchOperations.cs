using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace WebAPISamplePrototype
{
    public class BatchOperations
    {
        
        //List of Uris for records created in this sample
        static readonly List<Uri> entityUris = new List<Uri>();
        public static void Run(CDSWebApiService svc, bool deleteCreatedRecords)
        {
            Console.WriteLine("--Starting BatchOperations sample--");
            //Create an account outside of the batch to show linking.
            var parentAccount = new JObject
            {
                { "name", "Parent Account" }
            };
            Uri accountUri = svc.PostCreate("accounts", parentAccount);
            // Not technically necessary to convert to a relative Uri, but 
            // it does make the JSON passed more readable in Fiddler
            var relativeAccountUri = svc.BaseAddress.MakeRelativeUri(accountUri);
            entityUris.Add(relativeAccountUri);

            Console.WriteLine("\nCreated an account record before the batch operation.");

            //Create a BatchChangeSet to include operations within the batch
            var changeset = new BatchChangeSet();

            //Compose the body for the first contact.
            var contact1 = new JObject
            {
                { "firstname", "Joe" },
                { "lastname", "Smith" },
                { "accountrolecode", 1 }, //Decision Maker
                { "jobtitle","Manager" },
                //Related to the account
                { "parentcustomerid_account@odata.bind",relativeAccountUri.ToString() }
            };

            //Add a request to the changeset.
            changeset.Requests.Add(
                new HttpRequestMessage(HttpMethod.Post, "contacts")
                { Content = new StringContent(contact1.ToString()) }
                );
            
            //Compose the body for the second contact.
            var contact2 = new JObject
            {
                { "firstname", "Jack" },
                { "lastname", "Jones" },
                { "accountrolecode", 2 }, //Employee
                { "jobtitle", "Assistant" },
                //Related to first contact in batch
                { "parentcustomerid_contact@odata.bind", "$1" }, 
                //Related to the account
                { "parentcustomerid_account@odata.bind",relativeAccountUri.ToString() }
            };

            //Add another request to the changeset.
            changeset.Requests.Add(
                new HttpRequestMessage(HttpMethod.Post, "contacts")
                { Content = new StringContent(contact2.ToString()) }
                );

            //Add the changeset to the list of BatchItems
            var items = new List<BatchItem>() { changeset };

            var includeAnnotationsHeader = new Dictionary<string, string>();
            //Adding annotations to show formatted values
            includeAnnotationsHeader.Add("Prefer", "odata.include-annotations=\"*\"");

            var query = "?$select=name&$expand=contact_customer_accounts($select=fullname,accountrolecode)";
            var getRequest = new BatchGetRequest()
            {
                Path = $"{relativeAccountUri.ToString()}{query}",
                Headers = includeAnnotationsHeader
            };

            //Add the GET request to the list of BatchItems
            items.Add(getRequest);

            //Send the batch request.
            var responses = svc.PostBatch(items);

            Console.WriteLine("\nCreated these contact records in the batch:");
            responses.ForEach(x => {
                if (x.Headers.Contains("OData-EntityId")) {

                var contactRelativeUri =  svc.BaseAddress.MakeRelativeUri(new Uri(x.Headers.GetValues("OData-EntityId").FirstOrDefault()));
                    Console.WriteLine($"\tContact: {contactRelativeUri.ToString()}");

                }
            });

            Console.WriteLine("\nInformation about the Account retrieved in the batch:");
            Console.WriteLine(JObject.Parse(responses[2].Content.ReadAsStringAsync().Result));

            if (deleteCreatedRecords) {
                svc.Delete(relativeAccountUri);
                Console.WriteLine("\nThe account created for this sample was deleted and the related contacts with it.");
            }

            Console.WriteLine("--BatchOperations sample complete--");

        }
    }
}
