
using WebAPISamplePrototype.QueryExpressionTypes;
using Newtonsoft.Json;
using System;

namespace WebAPISamplePrototype
{
    public class QueryExpressionQuery
    {
        public static void Run(CDSWebApiService svc)
        {

            var qe = new QueryExpression("account")
            {
                
                ColumnSet = new ColumnSet("name"),
                PageInfo = new PagingInfo { Count = 3, PageNumber = 1 },
                Criteria = new FilterExpression(LogicalOperator.And)

            };
            qe.Criteria.AddCondition("name", ConditionOperator.BeginsWith, "Contoso");

            var qeObj = JsonConvert.SerializeObject(qe);

          var results =  svc.Get($"accounts?queryExpression={qeObj.ToString()}");

            Console.WriteLine("done");

        }
    }
}