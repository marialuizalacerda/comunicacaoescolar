﻿using App_comunicacao_escolar.Controllers;
using App_comunicacao_escolar.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace App_comunicacao_escolar.ViewComponents
{
    public class ContadorDeNotificacoesNovasViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public ContadorDeNotificacoesNovasViewComponent(ApplicationDbContext context)
        {
            _context = context;

        }
        public async Task<IViewComponentResult> InvokeAsync(int idUsuarioLogado = -1)
        {
            int numeroContador = 0;
            if (_context.NumeroDeNovasMensagensNaConversa != null && _context.UsuariosQueArquivaramConversa != null) { 
                var mensagensNaoLidasDoUsuarioAtual = _context.NumeroDeNovasMensagensNaConversa.Where(n => n.UsuarioId == idUsuarioLogado);
                
                var mensagensArquivadasDoUsuarioAtual = _context.UsuariosQueArquivaramConversa.Where(u => u.UsuarioId == idUsuarioLogado);
                foreach (var item in mensagensNaoLidasDoUsuarioAtual)
                {
                    if (!mensagensArquivadasDoUsuarioAtual.Any(m => m.ConversaId == item.ConversaId))
                    {
                        numeroContador += item.NumeroDeMensagensNaoLidas;
                    }
                }
            }
            ViewBag.NumeroContador = 80;
            
            return View();
        }

    }
}
