using RoutingAI.DataContracts;
using RoutingAI.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoutingAI.Librarian
{
    public class LibrarianService : ILibrarianService
    {

        public void InitializeKMeans(Guid id)
        {
            throw new NotImplementedException();
        }

        public void UpdateCATableEntry(Guid id, int iteration, int index, libWyvernzora.Pair<Task, int> data)
        {
            throw new NotImplementedException();
        }

        public List<libWyvernzora.Pair<int, int>[]> GetCATable(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
