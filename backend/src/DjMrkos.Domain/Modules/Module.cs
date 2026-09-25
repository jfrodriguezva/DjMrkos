using DjMrkos.Domain.Common;

namespace DjMrkos.Domain.Modules;

/// <summary>
/// A top-level entry of the configurable menu (e.g. "Luces", "Música", "Cabina").
/// Modules are created and reordered by the DJ from the admin panel; the public
/// menu is nothing more than a query over the active modules.
/// </summary>
public sealed class Module : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Icon { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private Module() { }

    public static Module Create(string name, string? icon, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El módulo necesita un nombre.");

        return new Module
        {
            Name = name.Trim(),
            Slug = Slugify(name),
            Icon = icon,
            DisplayOrder = displayOrder,
            IsActive = true,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>Rehydrates a module coming back from storage; keeps invariants out of the repository layer.</summary>
    public static Module Rehydrate(Guid id, string name, string slug, string? icon, int displayOrder, bool isActive, DateTimeOffset createdAtUtc)
        => new()
        {
            Id = id,
            Name = name,
            Slug = slug,
            Icon = icon,
            DisplayOrder = displayOrder,
            IsActive = isActive,
            CreatedAtUtc = createdAtUtc,
        };

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El módulo necesita un nombre.");

        Name = name.Trim();
        Slug = Slugify(name);
    }

    public void SetIcon(string? icon) => Icon = icon;

    public void MoveTo(int displayOrder) => DisplayOrder = displayOrder;

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    internal static string Slugify(string value)
    {
        var builder = new System.Text.StringBuilder(value.Length);
        var lastWasDash = false;
        foreach (var ch in value.Trim())
        {
            var c = char.ToLowerInvariant(ch);
            if (char.IsLetterOrDigit(c))
            {
                builder.Append(c);
                lastWasDash = false;
            }
            else if (!lastWasDash && builder.Length > 0)
            {
                builder.Append('-');
                lastWasDash = true;
            }
        }

        return builder.ToString().TrimEnd('-');
    }
}
