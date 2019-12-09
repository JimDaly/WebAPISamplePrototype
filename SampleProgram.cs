using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;

namespace WebAPISamplePrototype
{
    class SampleProgram
    {
        static readonly string ApiUrl = "https://crmue.crm.dynamics.com"; //Get From App.Config
        //static readonly string ApiUrl = "https://apithrottletest.crm10.dynamics.com"; //Get From App.Config        

        static void Main()
        {
            try
            {
                using (CDSWebApiService svc = new CDSWebApiService(ApiUrl, GetAccessToken(ApiUrl)))
                {
                    BasicOperations.Run(svc);
                    ConditionalOperations.Run(svc);
                    FunctionsAndActions.Run(svc);
                    //QueryData.Run(svc);
                    //ThrottleTest.Run(svc);
                }

            }
            catch (CDSWebApiException ex)
            {
                Console.WriteLine($"Unexpected Error:\t{ex.Message}\n" +
                    $"\tStatusCode: {ex.StatusCode}\n" +
                    $"\tReasonPhrase: {ex.ReasonPhrase}\n" +
                    $"\tErrorCode: {ex.ErrorCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected Error:\t{0}", ex.Message);
            }
            finally {
                Console.WriteLine("Press any key to exit");
                Console.ReadLine();
            }

        }



        internal static string GetAccessToken(string AppUrl)
        {

            //This ClientId and RedirectUri are the ones shipped with sample code and are pre-approved for all CDS & Dyn365 CE environments
            string ClientId = "51f81489-12ee-4a9e-aaae-a2591f45987d";
            var RedirectUri = new Uri("app://58145B91-0C36-4500-8554-080854F2AC97");
            string Authority = "https://login.microsoftonline.com/common";

            //Standard ADAL Connection code
            var Context = new AuthenticationContext(Authority, false);

            AuthenticationResult AuthResult;
            //In .NET Framework solution will display the ADAL prompt window each time you run.
            try
            {
                 AuthResult = Context.AcquireTokenAsync(AppUrl,
                  ClientId,
                  RedirectUri,
                  new PlatformParameters(PromptBehavior.SelectAccount),
                  UserIdentifier.AnyUser)
                  .Result;
            }
            catch (Exception)
            {
                throw new Exception("The user closed the Sign-in dialog.");
            }


            return AuthResult.AccessToken;
        }
    }
}
