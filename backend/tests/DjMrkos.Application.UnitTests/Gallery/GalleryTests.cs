using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Application.Gallery;
using DjMrkos.Domain.Gallery;
using NSubstitute;
using Xunit;

namespace DjMrkos.Application.UnitTests.Gallery;

public sealed class GalleryTests
{
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00];
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D];

    [Theory]
    [InlineData("Boda Ana & Luis — Cuernavaca", "boda-ana-luis-cuernavaca")]
    [InlineData("  XV Años de Sofía  ", "xv-anos-de-sofia")]
    [InlineData("¡¡¡", "album")]
    public void ToSlug_ProducesUrlSafeLowercaseText(string title, string expected) =>
        Assert.Equal(expected, GalleryAlbum.ToSlug(title));

    [Fact]
    public async Task CreateAlbum_WhenSlugIsTaken_AppendsANumber()
    {
        var repository = Substitute.For<IGalleryRepository>();
        repository.SlugExistsAsync("boda", Arg.Any<CancellationToken>()).Returns(true);
        repository.SlugExistsAsync("boda-2", Arg.Any<CancellationToken>()).Returns(true);
        repository.SlugExistsAsync("boda-3", Arg.Any<CancellationToken>()).Returns(false);

        var created = await new CreateGalleryAlbumCommandHandler(repository)
            .Handle(new CreateGalleryAlbumCommand("Boda", null, null), CancellationToken.None);

        Assert.Equal("boda-3", created.Slug);
        Assert.False(created.IsPublished);
    }

    [Fact]
    public void AddImageValidator_RejectsFilesThatAreNotJpeg_EvenIfTheyClaimToBe()
    {
        var validator = new AddGalleryImageCommandValidator();

        Assert.True(validator.Validate(new AddGalleryImageCommand(Guid.NewGuid(), null, 1920, 1080, Jpeg, Jpeg)).IsValid);
        Assert.False(validator.Validate(new AddGalleryImageCommand(Guid.NewGuid(), null, 1920, 1080, Png, Jpeg)).IsValid);
        Assert.False(validator.Validate(new AddGalleryImageCommand(Guid.NewGuid(), null, 1920, 1080, Jpeg, [])).IsValid);
    }

    [Fact]
    public async Task AddImage_WhenTheDatabaseInsertFails_RemovesTheSavedFiles()
    {
        var repository = Substitute.For<IGalleryRepository>();
        var storage = Substitute.For<IGalleryImageStorage>();
        var album = GalleryAlbum.Create("Boda", "boda", null, null);
        repository.GetAlbumByIdAsync(album.Id, Arg.Any<CancellationToken>()).Returns(album);
        repository.AddImageAsync(Arg.Any<GalleryImage>(), Arg.Any<CancellationToken>()).Returns<Task>(_ => throw new InvalidOperationException("db down"));

        var sut = new AddGalleryImageCommandHandler(repository, storage);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.Handle(new AddGalleryImageCommand(album.Id, null, 1920, 1080, Jpeg, Jpeg), CancellationToken.None));
        storage.Received(1).Delete(Arg.Any<Guid>());
    }
}
