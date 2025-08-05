using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilkChillar.Domain.Entities;

public class ChillarReceive
{
    public int ReceiveId { get; set; }
    public int TenantId { get; set; }
    public DateOnly Date { get; set; }
    public string TimeOfDay { get; set; } = default!; // "morning" or "evening"

    public int ChillarId { get; set; }
    public int ChillarInchargeId { get; set; }
    public int DodhiId { get; set; }
    public int AddedBy { get; set; }

    public decimal GrossLiters { get; set; }
    public decimal? LR { get; set; }
    public decimal? Fat { get; set; }
    public decimal NetLiters { get; set; }

    public Tenant Tenant { get; set; } = default!;
    public Chillar Chillar { get; set; } = default!;
    public Employee ChillarIncharge { get; set; } = default!;
    public Employee Dodhi { get; set; } = default!;
    public User AddedByUser { get; set; } = default!;
}


