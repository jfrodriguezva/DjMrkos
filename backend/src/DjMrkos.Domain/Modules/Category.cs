using DjMrkos.Domain.Common;

namespace DjMrkos.Domain.Modules;

/// <summary>
/// A child of a <see cref="Module"/> (e.g. "Torre de luces" under "Luces").
/// Categories are always scoped to exactly one module — this is the one
/// hierarchy rule the whole configurable-menu feature is built around.
/// </summary>
public sealed class Category : Entity
{
    public Guid ModuleId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }

    /// <summary>
    /// "Desde" price shown in the quote builder cart, in MXN. Null means the category isn't
    /// individually priced (e.g. it's bundled with hiring the DJ) — the quote builder shows
    /// "cotización" for it instead of a number and it can't be dragged into the cart.
    /// </summary>
    public decimal? Price { get; private set; }

    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private Category() { }

    public static Category Create(Guid moduleId, string name, string? description, string? imageUrl, decimal? price, int displayOrder)
    {
        if (moduleId == Guid.Empty)
            throw new DomainException("La categoría debe pertenecer a un módulo.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("La categoría necesita un nombre.");
        if (price is < 0)
            throw new DomainException("El precio no puede ser negativo.");

        return new Category
        {
            ModuleId = moduleId,
            Name = name.Trim(),
            Slug = Module.Slugify(name),
            Description = description,
            ImageUrl = imageUrl,
            Price = price,
            DisplayOrder = displayOrder,
            IsActive = true,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };
    }

    public static Category Rehydrate(
        Guid id, Guid moduleId, string name, string slug, string? description,
        string? imageUrl, decimal? price, int displayOrder, bool isActive, DateTimeOffset createdAtUtc)
        => new()
        {
            Id = id,
            ModuleId = moduleId,
            Name = name,
            Slug = slug,
            Description = description,
            ImageUrl = imageUrl,
            Price = price,
            DisplayOrder = displayOrder,
            IsActive = isActive,
            CreatedAtUtc = createdAtUtc,
        };

    public void Update(string name, string? description, string? imageUrl, decimal? price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("La categoría necesita un nombre.");
        if (price is < 0)
            throw new DomainException("El precio no puede ser negativo.");

        Name = name.Trim();
        Slug = Module.Slugify(name);
        Description = description;
        ImageUrl = imageUrl;
        Price = price;
    }

    public void MoveTo(int displayOrder) => DisplayOrder = displayOrder;

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
