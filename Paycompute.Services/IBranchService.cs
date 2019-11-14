using Paycompute.Entity;
using System.Collections.Generic;

namespace Paycompute.Services
{
    public interface IBranchService
    {
        IEnumerable<Branch> GetAll();
    }
}
