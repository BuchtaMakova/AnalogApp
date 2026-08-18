using AnalogHub.Application.Common.Models;
using AnalogHub.Application.Photos.Commands;
using AnalogHub.Application.Photos.Commands.AnalyzePhoto;
using AnalogHub.Application.Photos.Commands.RegisterPhoto;
using AnalogHub.Application.Photos.Commands.RequestPhotoUploadUrl;
using AnalogHub.Application.Photos.Dtos;
using AnalogHub.Application.Photos.Queries;
using AnalogHub.Application.Tags.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AnalogHub.Api.Controllers;

[ApiController]
[Route("api/photos")]
public sealed class PhotosController : ControllerBase
{
    private readonly ISender _sender;

    public PhotosController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Paginated, filterable listing that backs the library grid's infinite scroll.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PhotoListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PhotoListItemDto>>> GetPhotos(
        [FromQuery] Guid? filmRollId,
        [FromQuery] Guid? cameraBodyId,
        [FromQuery] Guid? lensId,
        [FromQuery] Guid? flashId,
        [FromQuery] string? tag,
        [FromQuery] byte? minRating,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 60,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPhotosQuery(filmRollId, cameraBodyId, lensId, flashId, tag, minRating, page, pageSize);
        return Ok(await _sender.Send(query, cancellationToken));
    }

    /// <summary>Full detail for the lightbox: resolved image URLs, EXIF, AI critique, tags and albums.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PhotoDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PhotoDetailDto>> GetPhoto(Guid id, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetPhotoByIdQuery(id), cancellationToken));

    /// <summary>Step 1: obtain a presigned URL to upload the original scan directly to storage.</summary>
    [HttpPost("upload-url")]
    [ProducesResponseType(typeof(RequestPhotoUploadUrlResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<RequestPhotoUploadUrlResult>> RequestUploadUrl(
        [FromBody] RequestPhotoUploadUrlCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>Step 2: register the uploaded file as a Photo and enqueue derivative generation.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PhotoDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<PhotoDto>> Register(
        [FromBody] RegisterPhotoCommand command,
        CancellationToken cancellationToken)
    {
        var photo = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Register), new { id = photo.Id }, photo);
    }

    /// <summary>Runs the AI vision critique (composition/light/posing/tips) and attaches suggested tags.</summary>
    [HttpPost("{id:guid}/analyze")]
    [ProducesResponseType(typeof(AnalyzePhotoResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<AnalyzePhotoResult>> Analyze(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new AnalyzePhotoCommand(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>Sets the 0-5 star rating.</summary>
    [HttpPut("{id:guid}/rating")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateRating(Guid id, [FromBody] UpdateRatingRequest request, CancellationToken cancellationToken)
    {
        await _sender.Send(new UpdatePhotoRatingCommand(id, request.Rating), cancellationToken);
        return NoContent();
    }

    /// <summary>Sets the non-destructive display rotation (0/90/180/270°) for scans that came in sideways.</summary>
    [HttpPut("{id:guid}/rotation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateRotation(Guid id, [FromBody] UpdateRotationRequest request, CancellationToken cancellationToken)
    {
        await _sender.Send(new UpdatePhotoRotationCommand(id, request.RotationDegrees), cancellationToken);
        return NoContent();
    }

    /// <summary>Presigned, force-download URLs for a batch of photos — the "download separately" bulk action.</summary>
    [HttpPost("download-urls")]
    [ProducesResponseType(typeof(IReadOnlyList<PhotoDownloadLinkDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PhotoDownloadLinkDto>>> GetDownloadUrls(
        [FromBody] GetPhotoDownloadUrlsQuery query, CancellationToken cancellationToken)
        => Ok(await _sender.Send(query, cancellationToken));

    /// <summary>Bundles the selected photos' originals into a ZIP — the "download as ZIP" bulk action.</summary>
    [HttpPost("download-zip")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
    public async Task<IActionResult> DownloadZip([FromBody] DownloadPhotosAsZipQuery query, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(query, cancellationToken);
        return File(result.Content, "application/zip", result.FileName);
    }

    /// <summary>Attaches a tag by name (find-or-create), idempotent if already linked.</summary>
    [HttpPost("{id:guid}/tags")]
    [ProducesResponseType(typeof(PhotoTagDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PhotoTagDto>> AddTag(Guid id, [FromBody] AddTagRequest request, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new AddPhotoTagCommand(id, request.TagName), cancellationToken));

    [HttpDelete("{id:guid}/tags/{tagId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveTag(Guid id, Guid tagId, CancellationToken cancellationToken)
    {
        await _sender.Send(new RemovePhotoTagCommand(id, tagId), cancellationToken);
        return NoContent();
    }

    /// <summary>Deletes the photo and its original/preview/thumbnail objects from storage.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeletePhotoCommand(id), cancellationToken);
        return NoContent();
    }

    public sealed record UpdateRatingRequest(byte Rating);

    public sealed record UpdateRotationRequest(int RotationDegrees);

    public sealed record AddTagRequest(string TagName);
}
