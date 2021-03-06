// <auto-generated />
using System;
using DataLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DataLayer.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.8");

            modelBuilder.Entity("DataModels.Models.Actuacio", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AlumneId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("CentreAlMomentDeLactuacioId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("CursActuacioId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("DescripcioActuacio")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int?>("EtapaAlMomentDeLactuacioId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MinutsDuradaActuacio")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("MomentDeLactuacio")
                        .HasColumnType("TEXT");

                    b.Property<string>("NivellAlMomentDeLactuacio")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ObservacionsTipusActuacio")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("TipusActuacioId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("AlumneId");

                    b.HasIndex("CentreAlMomentDeLactuacioId");

                    b.HasIndex("CursActuacioId");

                    b.HasIndex("EtapaAlMomentDeLactuacioId");

                    b.HasIndex("TipusActuacioId");

                    b.ToTable("Actuacions");
                });

            modelBuilder.Entity("DataModels.Models.Alumne", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("CentreActualId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Cognoms")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("CursDarreraActualitacioDadesId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("DataDarreraActuacio")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DataDarreraModificacio")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DataInformeNESENEE")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DataInformeNESENoNEE")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DataNaixement")
                        .HasColumnType("TEXT");

                    b.Property<bool>("EsActiu")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("EtapaActualId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("NivellActual")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("NombreTotalDactuacions")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ObservacionsNESENEE")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ObservacionsNESENoNEE")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Tags")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CentreActualId");

                    b.HasIndex("CursDarreraActualitacioDadesId");

                    b.HasIndex("EtapaActualId");

                    b.ToTable("Alumnes");
                });

            modelBuilder.Entity("DataModels.Models.Centre", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Codi")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("EsActiu")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Centres");
                });

            modelBuilder.Entity("DataModels.Models.CursAcademic", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AnyInici")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("EsActiu")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("CursosAcademics");
                });

            modelBuilder.Entity("DataModels.Models.Etapa", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Codi")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("EsActiu")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("SonEstudisObligatoris")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Etapes");
                });

            modelBuilder.Entity("DataModels.Models.TipusActuacio", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Codi")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("EsActiu")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("TipusActuacions");
                });

            modelBuilder.Entity("DataModels.Models.Actuacio", b =>
                {
                    b.HasOne("DataModels.Models.Alumne", "Alumne")
                        .WithMany("Actuacions")
                        .HasForeignKey("AlumneId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DataModels.Models.Centre", "CentreAlMomentDeLactuacio")
                        .WithMany()
                        .HasForeignKey("CentreAlMomentDeLactuacioId");

                    b.HasOne("DataModels.Models.CursAcademic", "CursActuacio")
                        .WithMany("Actuacions")
                        .HasForeignKey("CursActuacioId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DataModels.Models.Etapa", "EtapaAlMomentDeLactuacio")
                        .WithMany()
                        .HasForeignKey("EtapaAlMomentDeLactuacioId");

                    b.HasOne("DataModels.Models.TipusActuacio", "TipusActuacio")
                        .WithMany("Actuacions")
                        .HasForeignKey("TipusActuacioId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Alumne");

                    b.Navigation("CentreAlMomentDeLactuacio");

                    b.Navigation("CursActuacio");

                    b.Navigation("EtapaAlMomentDeLactuacio");

                    b.Navigation("TipusActuacio");
                });

            modelBuilder.Entity("DataModels.Models.Alumne", b =>
                {
                    b.HasOne("DataModels.Models.Centre", "CentreActual")
                        .WithMany("Alumnes")
                        .HasForeignKey("CentreActualId");

                    b.HasOne("DataModels.Models.CursAcademic", "CursDarreraActualitacioDades")
                        .WithMany("AlumnesActualitzats")
                        .HasForeignKey("CursDarreraActualitacioDadesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DataModels.Models.Etapa", "EtapaActual")
                        .WithMany("Alumnes")
                        .HasForeignKey("EtapaActualId");

                    b.Navigation("CentreActual");

                    b.Navigation("CursDarreraActualitacioDades");

                    b.Navigation("EtapaActual");
                });

            modelBuilder.Entity("DataModels.Models.Alumne", b =>
                {
                    b.Navigation("Actuacions");
                });

            modelBuilder.Entity("DataModels.Models.Centre", b =>
                {
                    b.Navigation("Alumnes");
                });

            modelBuilder.Entity("DataModels.Models.CursAcademic", b =>
                {
                    b.Navigation("Actuacions");

                    b.Navigation("AlumnesActualitzats");
                });

            modelBuilder.Entity("DataModels.Models.Etapa", b =>
                {
                    b.Navigation("Alumnes");
                });

            modelBuilder.Entity("DataModels.Models.TipusActuacio", b =>
                {
                    b.Navigation("Actuacions");
                });
#pragma warning restore 612, 618
        }
    }
}
