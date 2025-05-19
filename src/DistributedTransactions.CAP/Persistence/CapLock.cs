using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetCorePal.Extensions.DistributedTransactions.CAP.Persistence;

public class CapLock
{
    public string Key { get; set; } = null!;
    public string? Instance { get; set; }

    public DateTime? LastLockTime { get; set; }
}