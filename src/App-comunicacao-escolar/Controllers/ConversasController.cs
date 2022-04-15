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

namespace App_comunicacao_escolar.Controllers
{
    public class ConversasController : CommonController
    {
        private readonly ApplicationDbContext _context;

        public ConversasController(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        // GET: Conversas
        public async Task<IActionResult> Index(string? searchString, string? secao)
        {
            int idDoUsuarioLogado = GetIdUsuarioLogado();

            var applicationDbContext = await Task.Run(() => _context.Conversas.Include(c => c.Participantes).Include(c => c.NumeroDeNovasMensagensNaConversa));

            var conversas = await Task.Run(() => from c in applicationDbContext select c);

            conversas = await Task.Run(() => conversas.Where(d => d.Participantes.Any(p => p.Id == idDoUsuarioLogado) || d.RemetenteId == idDoUsuarioLogado));

            if (searchString != null)
            {
                conversas = await Task.Run(() => conversas.Where(d => d.Assunto.Contains(searchString) || d.Mensagens.Any(m => m.Conteudo.Contains(searchString))));
            }
            ViewBag.IdUsuarioLogado = idDoUsuarioLogado;
            return View(conversas);
        }

        // GET: Conversas/Visualizar/5
        public async Task<IActionResult> Visualizar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int idDoUsuarioLogado = GetIdUsuarioLogado();
            var applicationDbContext = await Task.Run(() => _context.Conversas.Include(c => c.Mensagens.Where(m => m.Participantes.Any(p => p.Id == idDoUsuarioLogado) || m.RemetenteId == idDoUsuarioLogado)).Include(c => c.Participantes));
            var conversa = await applicationDbContext
                .FirstOrDefaultAsync(c => c.Id == id);

            // Bloquear acesso de usuario via URL caso ele não seja participante da conversa.
            bool usuarioIsParticipanteDaConversa = conversa.Participantes.Any(p => p.Id == idDoUsuarioLogado) || conversa.RemetenteId == idDoUsuarioLogado;
            if (!usuarioIsParticipanteDaConversa)
            {
                return Forbid();
            }

            if (conversa == null)
            {
                return NotFound();
            }
            ViewData["ParticipanteId"] = new SelectList(_context.Usuarios, "Id", "Nome");

            // Zerar contador de mensagens da conversa
            var numeroDeNovasMensagensNaConversa = await _context.numeroDeNovasMensagensNaConversa.FirstOrDefaultAsync(n => n.UsuarioId == idDoUsuarioLogado && n.ConversaId == conversa.Id);
            if (numeroDeNovasMensagensNaConversa != null)
            {
                _context.numeroDeNovasMensagensNaConversa.Remove(numeroDeNovasMensagensNaConversa);
            }
            await _context.SaveChangesAsync();

            return View(conversa);
        }

        // GET: Conversas/Create
        public IActionResult Create()
        {
            ViewData["ParticipanteId"] = new SelectList(_context.Usuarios, "Id", "Nome");
            return View();
        }

        // POST: Conversas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Assunto,PrimeiraMensagem,RemetenteNome,RemetenteId")] Conversa conversa, [Bind("listaDeDestinatariosPorId")] string listaDeDestinatariosPorId, [Bind("conteudoMensagem")] string conteudoMensagem, Mensagem mensagem)
        {
            int idDoUsuarioLogado = GetIdUsuarioLogado();
            mensagem.DataEnvio = DateTime.Now;
            mensagem.Conteudo = conteudoMensagem;
            mensagem.RemetenteId = idDoUsuarioLogado;
            mensagem.RemetenteNome = await Task.Run(() => _context.Usuarios.FirstOrDefault(u => u.Id == idDoUsuarioLogado).Nome);

            conversa.PrimeiraMensagem = mensagem.Conteudo;
            conversa.RemetenteNome = mensagem.RemetenteNome;
            conversa.RemetenteId = mensagem.RemetenteId;

            if (ModelState.IsValid)
            {
                conversa.Participantes = new List<Usuario>();
                conversa.Mensagens = new List<Mensagem>();
                conversa.NumeroDeNovasMensagensNaConversa = new List<NumeroDeNovasMensagensNaConversa>();
                mensagem.Participantes = new List<Usuario>();
                mensagem.listaDestinatarios = listaDeDestinatariosPorId;

                List<string> listaRemetentes = listaDeDestinatariosPorId.Split(";").ToList();
        
                string listaDeDestinatariosPorNome = "";
                for (int i = 0; i < (listaRemetentes.Count - 1); i++)
                {
                    int remetenteId = int.Parse(listaRemetentes[i]);
                    Usuario usuario = await _context.Usuarios.FirstOrDefaultAsync(s => s.Id == remetenteId);
                    conversa.Participantes.Add(usuario);
                    mensagem.Participantes.Add(usuario);

                    NumeroDeNovasMensagensNaConversa numeroDeNovasMensagensNaConversa = new NumeroDeNovasMensagensNaConversa();
                    numeroDeNovasMensagensNaConversa.UsuarioId = usuario.Id;
                    numeroDeNovasMensagensNaConversa.NumeroDeMensagensNaoLidas = 1;
                    conversa.NumeroDeNovasMensagensNaConversa.Add(numeroDeNovasMensagensNaConversa);

                    listaDeDestinatariosPorNome += usuario.Nome + "; ";
                }

                mensagem.listaDestinatariosNome = listaDeDestinatariosPorNome;

                conversa.Mensagens.Add(mensagem);
                _context.Add(conversa);
                _context.Add(mensagem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(conversa);
        }

        // POST: Conversas/CreateResposta
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateResposta(Mensagem mensagem, [Bind("conteudoMensagem")] string conteudoMensagem, [Bind("conversaId")] int conversaId, [Bind("mensagemRespondidaId")] int mensagemRespondidaId, [Bind("listaDeDestinatariosPorId")] string listaDeDestinatariosPorId)
        {
            int idDoUsuarioLogado = GetIdUsuarioLogado();
            mensagem.ConversaId = conversaId;
            mensagem.MensagemRespondidaId = mensagemRespondidaId;
            mensagem.DataEnvio = DateTime.Now;
            mensagem.Conteudo = conteudoMensagem;
            mensagem.listaDestinatarios= listaDeDestinatariosPorId;
            mensagem.RemetenteId = idDoUsuarioLogado;
            mensagem.RemetenteNome = await Task.Run(() => _context.Usuarios.FirstOrDefault(u => u.Id == idDoUsuarioLogado).Nome);

            Conversa conversa = await Task.Run(() => _context.Conversas.Include(c => c.Participantes).Include(c => c.NumeroDeNovasMensagensNaConversa).FirstOrDefault(u => u.Id == conversaId));
            
            // Bloquear o usuario de postar em conversa da qual não faz parte via inspetor de código.
            bool usuarioIsParticipanteDaConversa = conversa.Participantes.Any(p => p.Id == idDoUsuarioLogado) || conversa.RemetenteId == idDoUsuarioLogado;

            if (ModelState.IsValid! && isValidCustomizado(mensagem) && usuarioIsParticipanteDaConversa)
            {
                mensagem.Participantes = new List<Usuario>();

                List<string> listaRemetentes = listaDeDestinatariosPorId.Split(";").ToList();

                string listaDeDestinatariosPorNome = "";
                for (int i = 0; i < (listaRemetentes.Count - 1); i++)
                {
                    int remetenteId = int.Parse(listaRemetentes[i]);
                    Usuario usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == remetenteId);
                    mensagem.Participantes.Add(usuario);

                    if (!conversa.Participantes.Contains(usuario))
                    {
                        conversa.Participantes.Add(usuario);

                    }

                    NumeroDeNovasMensagensNaConversa numeroDeNovasMensagensNaConversa = await Task.Run(() => _context.numeroDeNovasMensagensNaConversa.FirstOrDefault(n => n.UsuarioId == usuario.Id && n.ConversaId == conversaId));
                    if (numeroDeNovasMensagensNaConversa == null)
                    {
                        numeroDeNovasMensagensNaConversa = new NumeroDeNovasMensagensNaConversa();
                        numeroDeNovasMensagensNaConversa.UsuarioId = usuario.Id;
                        numeroDeNovasMensagensNaConversa.ConversaId = conversaId;
                        numeroDeNovasMensagensNaConversa.NumeroDeMensagensNaoLidas = 1;
                        conversa.NumeroDeNovasMensagensNaConversa.Add(numeroDeNovasMensagensNaConversa);
                    }
                    else
                    {
                        numeroDeNovasMensagensNaConversa.NumeroDeMensagensNaoLidas += 1;
                        _context.Update(numeroDeNovasMensagensNaConversa);
                    }
                    
                    listaDeDestinatariosPorNome += usuario.Nome + "; ";
                }
                mensagem.listaDestinatariosNome = listaDeDestinatariosPorNome;
                _context.Add(mensagem);
                _context.Update(conversa);
                await _context.SaveChangesAsync();
            }
            return RedirectToRoute(new { controller = "Conversas", action = "Visualizar", id = mensagem.ConversaId });
        }

        // Metodos

        private bool isValidCustomizado(Mensagem mensagem)
        {
            if (mensagem.listaDestinatarios == null){
                return false;
            };
            return true;
        }

    }
}
