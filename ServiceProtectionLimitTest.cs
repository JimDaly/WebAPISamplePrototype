using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace WebAPISamplePrototype
{
    public class ServiceProtectionLimitTest
    {
        /// <summary>
        /// Performs 3000 CRUD operations on multiple threads which should be enough to trigger Service Protection limits.
        /// </summary>
        /// <param name="svc"></param>
        public static void Run(CDSWebApiService svc) {


            ParallelLoopResult result = Parallel.For(0, 1000, ctr => {

                var contact = new JObject
                {
                    ["firstname"] = $"test",
                    ["lastname"] = $"contact {ctr}"
                };
               string name = $"{contact["lastname"]}";

                var contactUri = svc.PostCreate("contacts", contact);
                Console.WriteLine($"\t {name} Created");

                svc.Get($"{contactUri}?$select=lastname");
                Console.WriteLine($"\t {name} Retrieved");

                svc.Delete(contactUri);
                Console.WriteLine($"\t {name} Deleted");

            });

            Console.WriteLine("Result: {0}", result.IsCompleted ? 
                "Completed Normally" :
                $"Completed to {result.LowestBreakIteration}");

        }
    }
}
