using Microsoft.AspNetCore.Http;

namespace Application.Features.Images.DTOs;

public class UplodImageRequestControllerDTO
{
    public List<IFormFile> images { get; set; } = new();
}

public class UploadImageRequestControllerDTO : UplodImageRequestControllerDTO
{
}
