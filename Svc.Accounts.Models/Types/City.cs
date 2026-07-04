using System.ComponentModel.DataAnnotations;
using Z.EntityFramework.Plus;

namespace Svc.Accounts.Models.Types;

/// <summary>
/// City
/// </summary>
public class City
{
    /// <summary>
    /// Name.
    /// </summary>
    [MaxLength(128)]
    public virtual string? Name { get; set; }

    /// <summary>
    /// Name Normalized.
    /// </summary>
    [Required]
    [MaxLength(128)]
    [AuditExclude]
    public virtual string NameNormalized { get; internal set; } = null!;

    /// <summary>
    /// Zip Code.
    /// </summary>
    [MaxLength(32)]
    public virtual string? ZipCode { get; set; }
}