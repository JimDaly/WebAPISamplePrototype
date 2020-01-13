using System;
using System.Collections.Generic;
using System.Net.Http;

namespace WebAPISamplePrototype
{
    public abstract class BatchItem
    {

    }
    public class BatchChangeSet : BatchItem
    {

        public Guid Id { get; set; } = Guid.NewGuid();

        public List<HttpRequestMessage> Requests { get; set; } = new List<HttpRequestMessage>();
    }

    public class BatchGetRequest : BatchItem
    {
        public string Path { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
    }
}
