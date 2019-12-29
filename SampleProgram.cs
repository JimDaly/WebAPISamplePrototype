using System;
using System.Configuration;

namespace WebAPISamplePrototype
{
    public partial class SampleProgram
    {
        //Get configuration data from App.config connectionStrings
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["Connect"].ConnectionString;

        private static readonly string url = GetParameterValueFromConnectionString(connectionString, "Url");
        private static readonly string clientId = GetParameterValueFromConnectionString(connectionString, "ClientId");
        private static readonly string redirectUrl = GetParameterValueFromConnectionString(connectionString, "RedirectUrl");
        private static readonly string userPrincipalName = GetParameterValueFromConnectionString(connectionString, "UserPrincipalName");
        private static readonly string password = GetParameterValueFromConnectionString(connectionString, "Password");
        private static readonly string callerObjectId = GetParameterValueFromConnectionString(connectionString, "CallerObjectId");
        private static readonly string version = GetParameterValueFromConnectionString(connectionString, "Version");
        private static readonly int maxRetries = int.Parse(GetParameterValueFromConnectionString(connectionString, "MaxRetries"));
        private static readonly double timeoutInSeconds = double.Parse(GetParameterValueFromConnectionString(connectionString, "TimeoutInSeconds"));



        private static void Main()
        {
            try
            {
                using (CDSWebApiService svc = new CDSWebApiService(
                    url,
                    clientId,
                    redirectUrl,
                    userPrincipalName,
                    password,
                    callerObjectId,
                    version,
                    maxRetries,
                    timeoutInSeconds))
                {
                    BasicOperations.Run(svc, true);
                    ConditionalOperations.Run(svc);
                    FunctionsAndActions.Run(svc);
                    QueryData.Run(svc,true);
                   // QueryExpressionQuery.Run(svc);
                    // ServiceProtectionLimitTest.Run(svc);

                }
            }
            catch (CDSWebApiException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Unexpected Error:\t{ex.Message}\n" +
                    $"\tStatusCode: {ex.StatusCode}\n" +
                    $"\tReasonPhrase: {ex.ReasonPhrase}\n" +
                    $"\tErrorCode: {ex.ErrorCode}");
                Console.ResetColor();
            }
            catch (AggregateException aex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.BackgroundColor = ConsoleColor.White;
                Console.WriteLine("Unexpected Errors:\t{0}", aex.Message);
                foreach (Exception ex in aex.InnerExceptions)
                {
                    Console.WriteLine($"\t{ex.GetType().Name}: {ex.Message}");

                    if (ex.InnerException != null) {
                        Console.WriteLine($"\t\t{ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                    }

                }
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.BackgroundColor = ConsoleColor.White;
                Console.WriteLine("Unexpected Error:\t{0}", ex.Message);
                Console.ResetColor();
            }
            finally
            {
                Console.WriteLine("Press any key to exit");
                Console.ReadLine();
            }
        }
    }
}