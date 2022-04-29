﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App_comunicacao_escolar.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace App_comunicacao_escolar.Controllers
{
    public class UsuariosController : CommonController
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<IActionResult> Login()
        {
            // Cria usuário do tipo "Administrador" caso não haja nenhum registrado no sistema (para facilitar a realização dos testes)
            try {
                var usuarioAdmin = await _context.Usuarios.FirstOrDefaultAsync(u => u.Perfil == 0);
                if (usuarioAdmin == null) {
                    Usuario usuario = new();

                    usuario.Perfil = 0;

                    usuario.Nome = "Administrador";
                    usuario.Sobrenome = "do Sistema";
                    usuario.NomeDeUsuario = "admin";

                    usuario.NomeDisplayLista = usuario.Nome + " (" + usuario.NomeDeUsuario + ")";

                    usuario.Logradouro = "Vazio";
                    usuario.Cidade = "Vazio";
                    usuario.Estado = "XX";
                    usuario.Email = "Vazio";
                    usuario.Cep = "00000-000";

                    usuario.Senha = "admin";
                    usuario.Senha = BCrypt.Net.BCrypt.HashPassword(usuario.Senha);

                    _context.Add(usuario);
                    await _context.SaveChangesAsync();
                }
            }
            catch {
                return BadRequest();
            };
            // --------------------------------------------------------------------------------------------------------------------------
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([Bind("NomeDeUsuario,Senha")] Usuario usuario)
        {
            var user = await _context.Usuarios
                .FirstOrDefaultAsync(m => m.NomeDeUsuario == usuario.NomeDeUsuario);
            if (user == null)
            {
                ViewBag.Message = "Usuário e/ou Senha inválidos!";
                return View();
            }

            bool isSenhaOk = BCrypt.Net.BCrypt.Verify(usuario.Senha, user.Senha);

            if (isSenhaOk)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Nome),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Perfil.ToString())
                };

                var userIdentity = new ClaimsIdentity(claims, "login");

                ClaimsPrincipal principal = new(userIdentity);

                var props = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    ExpiresUtc = DateTime.UtcNow.ToLocalTime().AddDays(7),
                    IsPersistent = true
                };

                await HttpContext.SignInAsync(principal, props);

                return Redirect("/");
            }

            ViewBag.Message = "Usuário e/ou Senha inválidos!";
            return View();
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login", "Usuarios");
        }
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: Usuarios
        public async Task<IActionResult> Index(string searchString, string tipoUsuario)
        {
            try { 
                var applicationDbContext = _context.Usuarios;

                var usuarios = from u in applicationDbContext select u;

                usuarios = usuarios.OrderByDescending(u => u.Perfil == 0).ThenBy(u => u.NomeDisplayLista);

                if (searchString != null)
                {
                    usuarios = usuarios.Where(d => d.NomeDisplayLista.Contains(searchString));
                }
                if (tipoUsuario != null) {
                    int tipoUsuarioNumero = -1;
                    if (tipoUsuario.ToString().Equals("ResponsavelAluno"))
                        tipoUsuarioNumero = 1;
                    if (tipoUsuario.ToString().Equals("Professor"))
                        tipoUsuarioNumero = 2;
                    if (tipoUsuario.ToString().Equals("Outros"))
                        tipoUsuarioNumero = 3;
                    if (tipoUsuarioNumero != -1) { 
                        usuarios = usuarios.Where(d => d.Perfil == (PerfilUsuarioEnum) tipoUsuarioNumero);
                    }
                    ViewData["TipoDeUsuario"] = tipoUsuario.ToString();
                }
                else
                {
                    ViewData["TipoDeUsuario"] = "";
                }
                return View(await usuarios.ToListAsync());
            }
            catch
            {
                return BadRequest();
            }
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int? id, string tipoUsuario)
        {
            try { 
                if (id == null)
                {
                    return NotFound();
                }

                var usuario = await _context.Usuarios.Include(u => u.Professor).ThenInclude(p => p.Disciplinas)
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (usuario == null)
                {
                    return NotFound();
                }
                if (tipoUsuario != null) { 
                    ViewData["TipoUsuario"] = tipoUsuario.ToString();
                }
                return View(usuario);
            }
            catch
            {
                return BadRequest();
            }
        }

        // GET: Usuarios/Create
        public IActionResult Create(string tipoDeUsuario)
        {
            return View();
        }

        // POST: Usuarios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,Sobrenome,NomeDeUsuario,Senha,Email,TelefoneMovel,TelefoneFixo,Logradouro,Cidade,Estado,Cep,Perfil")] Usuario usuario, [Bind("professorFormacao")] string professorFormacao, [Bind("professorNivel")] NivelDoProfessorEnum professorNivel)
        {
            usuario = FormatarInputs(usuario);
            List<string> listarErrosDeValidacao = IsValidCustomizado(usuario);
            
            Professor professor = new();
            if (usuario.Perfil.ToString().Equals("Professor"))
            {   
                professor.Usuario = usuario;
                professor.Formacao = professorFormacao;
                professor.Nivel = professorNivel;
            }

            Responsavel responsavel = new();
            if (usuario.Perfil.ToString().Equals("ResponsavelAluno"))
            {
                responsavel.Usuario = usuario;
            }

            while (listarErrosDeValidacao.Count > 0)
            {
                ViewData["Error"] = "Error";
                ModelState.AddModelError(listarErrosDeValidacao[0], listarErrosDeValidacao[1]);
                ViewData[listarErrosDeValidacao[0]] = listarErrosDeValidacao[1];
                listarErrosDeValidacao.RemoveRange(0, 2);
            }
            if (ModelState.IsValid)
            {
                usuario.Senha = BCrypt.Net.BCrypt.HashPassword(usuario.Senha);
                usuario.NomeDisplayLista = usuario.Nome + " " + usuario.Sobrenome + " (" + usuario.NomeDeUsuario + ")";
                _context.Add(usuario);
                if (usuario.Perfil.ToString().Equals("Professor"))
                {
                    _context.Add(professor);
                }
                if (usuario.Perfil.ToString().Equals("ResponsavelAluno"))
                {
                    _context.Add(responsavel);
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios.Include(u => u.Professor).FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null)
            {
                return NotFound();
            }
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Sobrenome,NomeDeUsuario,Senha,Email,TelefoneMovel,TelefoneFixo,Logradouro,Cidade,Estado,Cep,Perfil")] Usuario usuarioNovasInformacoes, 
            [Bind("professorFormacao")] string professorFormacao, 
            [Bind("professorNivel")] NivelDoProfessorEnum professorNivel)
        {
            if (id != usuarioNovasInformacoes.Id)
            {
                return NotFound();
            }

            // Manter mesmo perfil e senha
            Usuario usuario = await _context.Usuarios.Include(u => u.Professor).FirstOrDefaultAsync(u => u.Id == id);
            usuario.Nome = usuarioNovasInformacoes.Nome;
            usuario.Sobrenome = usuarioNovasInformacoes.Sobrenome;
            usuario.NomeDeUsuario = usuarioNovasInformacoes.NomeDeUsuario;
            usuario.Email = usuarioNovasInformacoes.Email;
            usuario.TelefoneMovel = usuarioNovasInformacoes.TelefoneMovel;
            usuario.TelefoneFixo = usuarioNovasInformacoes.TelefoneFixo;
            usuario.Logradouro = usuarioNovasInformacoes.Logradouro;
            usuario.Cidade = usuarioNovasInformacoes.Cidade;
            usuario.Estado = usuarioNovasInformacoes.Estado;
            usuario.Cep = usuarioNovasInformacoes.Cep;
            if (usuario.Professor != null)
            {
                usuario.Professor.Nivel = professorNivel;
                usuario.Professor.Formacao = professorFormacao;
            }

            usuario = FormatarInputs(usuario);
            List<string> listarErrosDeValidacao = IsValidCustomizado(usuario, usuario.Id);
            while (listarErrosDeValidacao.Count > 0)
            {
                ViewData["Error"] = "Error";
                ModelState.AddModelError(listarErrosDeValidacao[0], listarErrosDeValidacao[1]);
                ViewData[listarErrosDeValidacao[0]] = listarErrosDeValidacao[1];
                listarErrosDeValidacao.RemoveRange(0, 2);
            }
            if (ModelState.IsValid)
            {
                try
                {
                    usuario.NomeDisplayLista = usuario.Nome + " " + usuario.Sobrenome + " (" + usuario.NomeDeUsuario + ")";
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> AlterarDados(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (id != GetIdUsuarioLogado())
            {
                return Forbid();
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            if (TempData.ContainsKey("MensagemDeSucesso"))
                ViewData["MensagemDeSucesso"] = TempData["MensagemDeSucesso"].ToString();

            return View(usuario);
        }

        // POST: Usuarios/AlterarDados/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarDados(int id, [Bind("Id,Nome,Sobrenome,NomeDeUsuario,Senha,Email,TelefoneMovel,TelefoneFixo,Logradouro,Cidade,Estado,Cep,Perfil")] Usuario usuarioNovasInformacoes,
            [Bind("NovaSenha")] string novaSenha, [Bind("NovaSenha")] string novaSenhaRepetir)
        {
            if (id != GetIdUsuarioLogado())
            {
                return Forbid();
            }

            Usuario usuario = await _context.Usuarios.FindAsync(id);

            var user = await _context.Usuarios
                .FirstOrDefaultAsync(m => m.NomeDeUsuario == usuario.NomeDeUsuario);

            bool isSenhaOk = false;
            if (usuarioNovasInformacoes.Senha != null) {
                isSenhaOk = BCrypt.Net.BCrypt.Verify(usuarioNovasInformacoes.Senha, user.Senha);
            }
            if (isSenhaOk)
            {
                usuario.Email = usuarioNovasInformacoes.Email;
                usuario.TelefoneMovel = usuarioNovasInformacoes.TelefoneMovel;
                usuario.TelefoneFixo = usuarioNovasInformacoes.TelefoneFixo;
                usuario.Logradouro = usuarioNovasInformacoes.Logradouro;
                usuario.Cidade = usuarioNovasInformacoes.Cidade;
                usuario.Estado = usuarioNovasInformacoes.Estado;
                usuario.Cep = usuarioNovasInformacoes.Cep;

            }
            else
            {
                ModelState.AddModelError("", "");
            }

            if (novaSenha != null) {
                if (novaSenha.Equals(novaSenhaRepetir) && isSenhaOk)
                {
                    usuario.Senha = BCrypt.Net.BCrypt.HashPassword(novaSenha);
                }
                else
                {
                    ModelState.AddModelError("", "");
                }
            }

            usuario = FormatarInputs(usuario);
            List<string> listarErrosDeValidacao = IsValidCustomizado(usuario, usuario.Id);
            while (listarErrosDeValidacao.Count > 0)
            {
                ViewData["Error"] = "Error";
                ModelState.AddModelError(listarErrosDeValidacao[0], listarErrosDeValidacao[1]);
                ViewData[listarErrosDeValidacao[0]] = listarErrosDeValidacao[1];
                listarErrosDeValidacao.RemoveRange(0, 2);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    usuario.NomeDisplayLista = usuario.Nome + " " + usuario.Sobrenome + " (" + usuario.NomeDeUsuario + ")";
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["MensagemDeSucesso"] = "Dados alterados com sucesso!";
                return RedirectToAction(nameof(AlterarDados));
            }
            ViewData["MensagemDeErro"] = "Dados não foram alterados!";
            return View(usuario);
        }

        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(m => m.Id == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario.Perfil.ToString().Equals("Admin"))
            {
                return Forbid("Não é permitido deletar a conta de usuário do Adminisrador!");
            }
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Métodos
        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }

        private Usuario FormatarInputs(Usuario usuario)
        {
            usuario.NomeDeUsuario = usuario.NomeDeUsuario.Trim();
            usuario.Email = usuario.Email.Trim();
            return usuario;
        }

        private List<string> IsValidCustomizado(Usuario usuario, int idUsuarioSendoAtualizado = 0)
        {
            List<string> errorMessage = new();
            if (_context.Usuarios.Any(x => x.NomeDeUsuario == usuario.NomeDeUsuario && x.Id != idUsuarioSendoAtualizado))
            {
                errorMessage.Add("NomeDeUsuario");
                errorMessage.Add("Nome de usuário já utilizado por outro usuário!");
            }
            if (_context.Usuarios.Any(x => x.Email == usuario.Email && x.Id != idUsuarioSendoAtualizado))
            {
                errorMessage.Add("Email");
                errorMessage.Add("E-mail já utilizado por outro usuário!");
            }
            if (usuario.Perfil.ToString().Equals("Admin") && idUsuarioSendoAtualizado == 0)
            {
                errorMessage.Add("Perfil");
                errorMessage.Add("Não é permitido criar um novo usuário do tipo \"Administrador\"!");
            }
            if (usuario.NomeDeUsuario == "admin" && !usuario.Perfil.ToString().Equals("Admin"))
            {
                errorMessage.Add("NomeDeUsuario");
                errorMessage.Add("Nome reservado!");
            }

            return errorMessage;
        }
    }
}
