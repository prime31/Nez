using System.Collections.Generic;

namespace Nez.Persistence.Binary
{
    public interface IPersistable
    {
        void RecoverLegacyData(string key, uint storedVersion, Dictionary<string,object> blob);
        void Recover(IPersistableReader reader);
        void Persist(IPersistableWriter writer);
    }
}