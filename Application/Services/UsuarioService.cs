using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IRepository<Pessoa> _pessoaRepository;
    private readonly IRepository<Usuario> _usuarioRepository;

    public UsuarioService(
        IRepository<Pessoa> pessoaRepository,
        IRepository<Usuario> usuarioRepository)
    {
        _pessoaRepository = pessoaRepository;
        _usuarioRepository = usuarioRepository;
    }

    public async Task<UsuarioResponseDto> CadastrarUsuarioAsync(CadastroUsuarioDto dto)
    {
        // Verificar se o login já existe
        var usuarioExistente = await _usuarioRepository.BuscarAsync(u => u.UsuLogin == dto.Login);
        if (usuarioExistente != null)
        {
            throw new InvalidOperationException("Login já está em uso. Por favor, escolha outro login.");
        }

        // Criar Pessoa
        var pessoa = new Pessoa
        {
            PesFantasia = $"{dto.Nome} {dto.Sobrenome}".Trim(),
            PesDocFederal = dto.DocFederal,
            PesDocEstadual = dto.DocEstadual
        };

        await _pessoaRepository.InserirAsync(pessoa);
        await _pessoaRepository.SalvarAlteracoesAsync();

        // Criar Usuario
        var usuario = new Usuario
        {
            PesId = pessoa.PesId,
            UsuLogin = dto.Login,
            UsuSenha = dto.Senha // Em produção, deve ser hash da senha
        };

        await _usuarioRepository.InserirAsync(usuario);
        await _usuarioRepository.SalvarAlteracoesAsync();

        // Separar fantasia em nome e sobrenome (assumindo que o primeiro espaço separa)
        var partesNome = (pessoa.PesFantasia ?? string.Empty).Split(' ', 2);
        return new UsuarioResponseDto
        {
            UsuarioId = usuario.UsuId,
            PessoaId = pessoa.PesId,
            Nome = partesNome.Length > 0 ? partesNome[0] : string.Empty,
            Sobrenome = partesNome.Length > 1 ? partesNome[1] : null,
            DocFederal = pessoa.PesDocFederal,
            DocEstadual = pessoa.PesDocEstadual,
            Login = usuario.UsuLogin ?? string.Empty
        };
    }

    public async Task<UsuarioResponseDto?> ObterUsuarioPorIdAsync(int id)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario == null)
            return null;

        var pessoa = await _pessoaRepository.GetByIdAsync(usuario.PesId);
        if (pessoa == null)
            return null;

        // Separar fantasia em nome e sobrenome (assumindo que o primeiro espaço separa)
        var partesNome = (pessoa.PesFantasia ?? string.Empty).Split(' ', 2);
        return new UsuarioResponseDto
        {
            UsuarioId = usuario.UsuId,
            PessoaId = pessoa.PesId,
            Nome = partesNome.Length > 0 ? partesNome[0] : string.Empty,
            Sobrenome = partesNome.Length > 1 ? partesNome[1] : null,
            DocFederal = pessoa.PesDocFederal,
            DocEstadual = pessoa.PesDocEstadual,
            Login = usuario.UsuLogin ?? string.Empty
        };
    }

    public async Task<UsuarioResponseDto?> ObterUsuarioPorLoginAsync(string login)
    {
        var usuario = await _usuarioRepository.BuscarAsync(u => u.UsuLogin == login);
        if (usuario == null)
            return null;

        var pessoa = await _pessoaRepository.GetByIdAsync(usuario.PesId);
        if (pessoa == null)
            return null;

        // Separar fantasia em nome e sobrenome (assumindo que o primeiro espaço separa)
        var partesNome = (pessoa.PesFantasia ?? string.Empty).Split(' ', 2);
        return new UsuarioResponseDto
        {
            UsuarioId = usuario.UsuId,
            PessoaId = pessoa.PesId,
            Nome = partesNome.Length > 0 ? partesNome[0] : string.Empty,
            Sobrenome = partesNome.Length > 1 ? partesNome[1] : null,
            DocFederal = pessoa.PesDocFederal,
            DocEstadual = pessoa.PesDocEstadual,
            Login = usuario.UsuLogin ?? string.Empty
        };
    }

    public async Task<IEnumerable<UsuarioResponseDto>> ListarTodosUsuariosAsync()
    {
        var usuarios = await _usuarioRepository.ListarTodosAsync();
        var resultado = new List<UsuarioResponseDto>();

        foreach (var usuario in usuarios)
        {
            var pessoa = await _pessoaRepository.GetByIdAsync(usuario.PesId);
            if (pessoa != null)
            {
                // Separar fantasia em nome e sobrenome (assumindo que o primeiro espaço separa)
                var partesNome = (pessoa.PesFantasia ?? string.Empty).Split(' ', 2);
                resultado.Add(new UsuarioResponseDto
                {
                    UsuarioId = usuario.UsuId,
                    PessoaId = pessoa.PesId,
                    Nome = partesNome.Length > 0 ? partesNome[0] : string.Empty,
                    Sobrenome = partesNome.Length > 1 ? partesNome[1] : null,
                    DocFederal = pessoa.PesDocFederal,
                    DocEstadual = pessoa.PesDocEstadual,
                    Login = usuario.UsuLogin ?? string.Empty
                });
            }
        }

        return resultado;
    }

    public async Task<LoginResponseDto> AutenticarAsync(LoginDto dto)
    {
        var usuario = await _usuarioRepository.BuscarAsync(u => u.UsuLogin == dto.Login);
        if (usuario == null || usuario.UsuSenha != dto.Senha)
        {
            throw new InvalidOperationException("Login ou senha inválidos.");
        }

        var pessoa = await _pessoaRepository.GetByIdAsync(usuario.PesId);
        if (pessoa == null)
        {
            throw new InvalidOperationException("Usuário não encontrado.");
        }

        return new LoginResponseDto
        {
            UsuarioId = usuario.UsuId,
            Login = usuario.UsuLogin ?? string.Empty,
            Nome = pessoa.PesFantasia ?? string.Empty,
            Autenticado = true
        };
    }
}
