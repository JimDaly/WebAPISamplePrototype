using System.Collections.Generic;

namespace Microsoft.Cds.Metadata.Query
{
    public class RetrieveMetadataChangesResponse
    {
        public DeletedMetadataCollection DeletedMetadata { get; set; }
        public List<ComplexEntityMetadata> EntityMetadata { get; set; }
        public string ServerVersionStamp { get; set; }
    }
}
