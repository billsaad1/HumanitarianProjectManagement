using HumanitarianProjectManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HumanitarianProjectManagement.Services
{
    public class PurchaseRequisitionService
    {
        private readonly HpmDbContext _context;

        public PurchaseRequisitionService(HpmDbContext context)
        {
            _context = context;
        }

        public async Task<List<PurchaseRequisition>> GetAllPurchaseRequisitionsAsync()
        {
            return await _context.PurchaseRequisitions.ToListAsync();
        }
    }
}
