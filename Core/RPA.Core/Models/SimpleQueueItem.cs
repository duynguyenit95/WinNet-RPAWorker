using System.Collections.Generic;

namespace RPA.Core
{
    public class SimpleQueueItem
    {
        public string Name { get; set; }
        public string Data { get; set; }
        public string ContextId { get; set; }

        public override string ToString()
        {
            return $"{Name}:{Data}:{ContextId}";
        }
    }
}
