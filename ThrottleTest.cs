using Newtonsoft.Json.Linq;
using System;

namespace WebAPISamplePrototype
{
    public class ThrottleTest
    {

        public static void Run(CDSWebApiService svc) {

            for (int i = 0; i < 100; i++)
            {
                var contact = new JObject
                {
                    ["firstname"] = $"test",
                    ["lastname"] = $"contact {i}"
                };

                Console.WriteLine($"{contact["lastname"]}");

                var contactUri = svc.PostCreate("contacts", contact);
                Console.WriteLine("\tCreated");

                svc.Get($"{contactUri}?$select=lastname");
                Console.WriteLine("\tRetrieved");

                svc.Delete(contactUri);
                Console.WriteLine("\tDeleted");

            }
        
        }
    }
}
