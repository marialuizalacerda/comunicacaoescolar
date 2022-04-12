﻿// <auto-generated />
using System;
using App_comunicacao_escolar.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace App_comunicacao_escolar.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20220412015827_M02")]
    partial class M02
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("App_comunicacao_escolar.Models.Conversa", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Assunto")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PrimeiraMensagem")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("RemetenteId")
                        .HasColumnType("int");

                    b.Property<string>("RemetenteNome")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Conversas");
                });

            modelBuilder.Entity("App_comunicacao_escolar.Models.Mensagem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Conteudo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ConversaId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("DataEnvio")
                        .HasColumnType("datetime2");

                    b.Property<int?>("MensagemRespondidaId")
                        .HasColumnType("int");

                    b.Property<int?>("RemetenteId")
                        .HasColumnType("int");

                    b.Property<string>("RemetenteNome")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("listaDestinatarios")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("listaDestinatariosNome")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("ConversaId");

                    b.HasIndex("MensagemRespondidaId");

                    b.ToTable("Mensagem");
                });

            modelBuilder.Entity("App_comunicacao_escolar.Models.NumeroDeNovasMensagensNaConversa", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int?>("ConversaId")
                        .HasColumnType("int");

                    b.Property<int>("NumeroDeMensagensNaoLidas")
                        .HasColumnType("int");

                    b.Property<bool>("PossuiMensagensNaoLidas")
                        .HasColumnType("bit");

                    b.Property<int>("UsuarioId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ConversaId");

                    b.ToTable("NumeroDeNovasMensagensNaConversa");
                });

            modelBuilder.Entity("App_comunicacao_escolar.Models.Usuario", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Cep")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Cidade")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Estado")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Logradouro")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NomeDeUsuario")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Perfil")
                        .HasColumnType("int");

                    b.Property<string>("Senha")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Sobrenome")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Usuarios");
                });

            modelBuilder.Entity("ConversaUsuario", b =>
                {
                    b.Property<int>("ConversasId")
                        .HasColumnType("int");

                    b.Property<int>("ParticipantesId")
                        .HasColumnType("int");

                    b.HasKey("ConversasId", "ParticipantesId");

                    b.HasIndex("ParticipantesId");

                    b.ToTable("ConversaUsuario");
                });

            modelBuilder.Entity("MensagemUsuario", b =>
                {
                    b.Property<int>("MensagemId")
                        .HasColumnType("int");

                    b.Property<int>("ParticipantesId")
                        .HasColumnType("int");

                    b.HasKey("MensagemId", "ParticipantesId");

                    b.HasIndex("ParticipantesId");

                    b.ToTable("MensagemUsuario");
                });

            modelBuilder.Entity("App_comunicacao_escolar.Models.Mensagem", b =>
                {
                    b.HasOne("App_comunicacao_escolar.Models.Conversa", "Conversa")
                        .WithMany("Mensagens")
                        .HasForeignKey("ConversaId");

                    b.HasOne("App_comunicacao_escolar.Models.Mensagem", "MensagemRespondida")
                        .WithMany("Respostas")
                        .HasForeignKey("MensagemRespondidaId");

                    b.Navigation("Conversa");

                    b.Navigation("MensagemRespondida");
                });

            modelBuilder.Entity("App_comunicacao_escolar.Models.NumeroDeNovasMensagensNaConversa", b =>
                {
                    b.HasOne("App_comunicacao_escolar.Models.Conversa", "Conversa")
                        .WithMany("NumeroDeNovasMensagensNaConversa")
                        .HasForeignKey("ConversaId");

                    b.Navigation("Conversa");
                });

            modelBuilder.Entity("ConversaUsuario", b =>
                {
                    b.HasOne("App_comunicacao_escolar.Models.Conversa", null)
                        .WithMany()
                        .HasForeignKey("ConversasId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("App_comunicacao_escolar.Models.Usuario", null)
                        .WithMany()
                        .HasForeignKey("ParticipantesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MensagemUsuario", b =>
                {
                    b.HasOne("App_comunicacao_escolar.Models.Mensagem", null)
                        .WithMany()
                        .HasForeignKey("MensagemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("App_comunicacao_escolar.Models.Usuario", null)
                        .WithMany()
                        .HasForeignKey("ParticipantesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("App_comunicacao_escolar.Models.Conversa", b =>
                {
                    b.Navigation("Mensagens");

                    b.Navigation("NumeroDeNovasMensagensNaConversa");
                });

            modelBuilder.Entity("App_comunicacao_escolar.Models.Mensagem", b =>
                {
                    b.Navigation("Respostas");
                });
#pragma warning restore 612, 618
        }
    }
}
