using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Configuration;

namespace WebAPISamplePrototype
{
    public partial class SampleProgram
    {
        //Get configuration data from App.config connectionStrings
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["Connect"].ConnectionString;

        private static readonly string Url = GetParameterValueFromConnectionString(connectionString, "Url");
        private static readonly string ClientId = GetParameterValueFromConnectionString(connectionString, "ClientId");
        private static readonly string RedirectUrl = GetParameterValueFromConnectionString(connectionString, "RedirectUrl");
        private static readonly string Username = GetParameterValueFromConnectionString(connectionString, "Username");
        private static readonly string Password = GetParameterValueFromConnectionString(connectionString, "Password");
        private static readonly string CallerObjectId = GetParameterValueFromConnectionString(connectionString, "CallerObjectId");
        private static readonly string Version = GetParameterValueFromConnectionString(connectionString, "Version");
        private static readonly int MaxRetries = int.Parse(GetParameterValueFromConnectionString(connectionString, "MaxRetries"));
        private static readonly double TimeoutInSeconds = double.Parse(GetParameterValueFromConnectionString(connectionString, "TimeoutInSeconds"));



        private static void Main()
        {
            try
            {
                using (CDSWebApiService svc = new CDSWebApiService(Url,
                    ClientId,
                    RedirectUrl,
                    Username,
                    Password,
                    CallerObjectId,
                    Version,
                    MaxRetries,
                    TimeoutInSeconds))
                {
                    BasicOperations.Run(svc);
                    //ConditionalOperations.Run(svc);
                    //FunctionsAndActions.Run(svc);
                    //QueryData.Run(svc);
                    //ServiceProtectionLimitTest.Run(svc);
                    //QueryExpressionQuery.Run(svc);
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