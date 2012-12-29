using libWyvernzora;
using RoutingAI.Algorithms.DFEKM;
using RoutingAI.ServiceContracts;
using System;
using System.Collections.Generic;

namespace RoutingAI.Librarian
{
    public class LibrarianService : ILibrarianService
    {
        Dictionary<Guid, CaTable> _tableStore = new Dictionary<Guid, CaTable>();

        #region 

        public void InitializeKMeans(Guid id, Pair<int, int>[] initCA)
        {
            throw new NotImplementedException();
        }

        public void InsertCATableRow(Guid id, Pair<int, int>[] row)
        {
            throw new NotImplementedException();
        }

        public List<Pair<int, int>[]> GetCATable(Guid id)
        {
            throw new NotImplementedException();
        }


        #endregion
    }
}
