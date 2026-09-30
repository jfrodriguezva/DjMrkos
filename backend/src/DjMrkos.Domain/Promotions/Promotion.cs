using DjMrkos.Domain.Common;

namespace DjMrkos.Domain.Promotions;

/// <summary>
/// A percentage discount the DJ configures for either one whole <see cref="Modules.Module"/>
/// (every priced category under it) or a single <see cref="Modules.Category"/> — never both,
/// never neither. The public menu applies it on top of <c>Category.Price</c>; a category with
/// no price ("incluido/a cotizar") has nothing to discount, so a promotion targeting it has no effect.
/// </summary>
public sealed class Promotion : Entity
{
    public Guid? ModuleId { get; private set; }
    public Guid? CategoryId { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public decimal DiscountPercentage { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private Promotion() { }

    public static Promotion Create(Guid? moduleId, Guid? categoryId, string label, decimal discountPercentage)
    {
        Validate(moduleId, categoryId, label, discountPercentage);

        return new Promotion
        {
            ModuleId = moduleId,
            CategoryId = categoryId,
            Label = label.Trim(),
            DiscountPercentage = discountPercentage,
            IsActive = true,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };
    }

    public static Promotion Rehydrate(
        Guid id, Guid? moduleId, Guid? categoryId, string label, decimal discountPercentage, bool isActive, DateTimeOffset createdAtUtc)
        => new()
        {
            Id = id,
            ModuleId = moduleId,
            CategoryId = categoryId,
            Label = label,
            DiscountPercentage = discountPercentage,
            IsActive = isActive,
            CreatedAtUtc = createdAtUtc,
        };

    public void Update(string label, decimal discountPercentage)
    {
        if (string.IsNullOrWhiteSpace(label))
            throw new DomainException("La promoción necesita un nombre.");
        if (discountPercentage is <= 0 or > 100)
            throw new DomainException("El descuento debe ser mayor a 0% y no puede superar 100%.");

        Label = label.Trim();
        DiscountPercentage = discountPercentage;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private static void Validate(Guid? moduleId, Guid? categoryId, string label, decimal discountPercentage)
    {
        var moduleSet = moduleId is not null && moduleId != Guid.Empty;
        var categorySet = categoryId is not null && categoryId != Guid.Empty;

        if (moduleSet == categorySet)
            throw new DomainException("La promoción debe aplicar exactamente a un módulo o a una categoría, no a ambos ni a ninguno.");
        if (string.IsNullOrWhiteSpace(label))
            throw new DomainException("La promoción necesita un nombre.");
        if (discountPercentage is <= 0 or > 100)
            throw new DomainException("El descuento debe ser mayor a 0% y no puede superar 100%.");
    }
}
