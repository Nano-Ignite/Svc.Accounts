using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DynamicExpression;
using Nano.App.Api.Controllers.Criteria;
using Svc.Accounts.Models.Data;
using Svc.Accounts.Models.Types;

namespace Svc.Accounts.Models.Criterias;

/// <inheritdoc />
public class AddressQueryCriteria : BaseQueryCriteria
{
    /// <summary>
    /// City Name.
    /// </summary>
    [MaxLength(128)]
    public virtual string? CityName { get; set; }

    /// <summary>
    /// City Zip Code.
    /// </summary>
    [MaxLength(128)]
    public virtual string? CityZipCode { get; set; }

    /// <summary>
    /// Street Name.
    /// </summary>
    [MaxLength(512)]
    public virtual string? StreetName { get; set; }

    /// <inheritdoc />
    public override IList<CriteriaExpression> GetExpressions()
    {
        var expressions = base.GetExpressions();

        var expression = new CriteriaExpression();

        if (!string.IsNullOrEmpty(this.CityName))
        {
            expression
                .StartsWith($"{nameof(Address.City)}.{nameof(City.NameNormalized)}", this.CityName.ToUpper());
        }

        if (!string.IsNullOrEmpty(this.CityZipCode))
        {
            expression
                .Equal($"{nameof(Address.City)}.{nameof(City.ZipCode)}", this.CityZipCode);
        }

        if (!string.IsNullOrEmpty(this.StreetName))
        {
            expression
                .StartsWith(nameof(Address.StreetNameNormalized), this.StreetName.ToUpper());
        }

        expressions
            .Add(expression);

        return expressions;
    }
}