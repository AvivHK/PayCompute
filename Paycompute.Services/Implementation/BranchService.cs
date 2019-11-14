using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Paycompute.Entity;
using Paycompute.Persistence;
namespace Paycompute.Services.Implementation
{
    public class BranchService : IBranchService
    {
        private readonly ApplicationDbContext _context;

        public BranchService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Branch> GetAll() => _context.Branches.AsNoTracking().OrderBy(br => br.Address);
    }
}
