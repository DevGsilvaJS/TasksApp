using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace TasksAppAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImagemController : ControllerBase
{
    private readonly IRepository<ImagemTarefa> _imagemRepository;
    private readonly ILogger<ImagemController> _logger;

    public ImagemController(
        IRepository<ImagemTarefa> imagemRepository,
        ILogger<ImagemController> logger)
    {
        _imagemRepository = imagemRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtém uma imagem por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterImagem(int id)
    {
        try
        {
            var imagem = await _imagemRepository.GetByIdAsync(id);
            if (imagem == null || imagem.ImgArquivo == null)
            {
                return NotFound();
            }

            // Determinar o tipo de conteúdo baseado no arquivo
            var contentType = "image/jpeg"; // padrão
            return File(imagem.ImgArquivo, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter imagem: {Id}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
}
