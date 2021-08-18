using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataLayer.Migrations
{
    public partial class inicial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Centres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Codi = table.Column<string>(type: "TEXT", nullable: false),
                    Nom = table.Column<string>(type: "TEXT", nullable: false),
                    EsActiu = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Centres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CursosAcademics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnyInici = table.Column<int>(type: "INTEGER", nullable: false),
                    Nom = table.Column<string>(type: "TEXT", nullable: false),
                    EsActiu = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CursosAcademics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Etapes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Codi = table.Column<string>(type: "TEXT", nullable: false),
                    Nom = table.Column<string>(type: "TEXT", nullable: false),
                    SonEstudisObligatoris = table.Column<bool>(type: "INTEGER", nullable: false),
                    EsActiu = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Etapes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TipusActuacions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Codi = table.Column<string>(type: "TEXT", nullable: false),
                    Nom = table.Column<string>(type: "TEXT", nullable: false),
                    EsActiu = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipusActuacions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Alumnes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", nullable: false),
                    Cognoms = table.Column<string>(type: "TEXT", nullable: false),
                    DataNaixement = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CentreActualId = table.Column<int>(type: "INTEGER", nullable: true),
                    CursDarreraActualitacioDadesId = table.Column<int>(type: "INTEGER", nullable: false),
                    EtapaActualId = table.Column<int>(type: "INTEGER", nullable: true),
                    DataInformeNESENEE = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ObservacionsNESENEE = table.Column<string>(type: "TEXT", nullable: false),
                    DataInformeNESENoNEE = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ObservacionsNESENoNEE = table.Column<string>(type: "TEXT", nullable: false),
                    Tags = table.Column<string>(type: "TEXT", nullable: false),
                    EsActiu = table.Column<bool>(type: "INTEGER", nullable: false),
                    NombreTotalDactuacions = table.Column<int>(type: "INTEGER", nullable: false),
                    DataDarreraActuacio = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DataDarreraModificacio = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alumnes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alumnes_Centres_CentreActualId",
                        column: x => x.CentreActualId,
                        principalTable: "Centres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Alumnes_CursosAcademics_CursDarreraActualitacioDadesId",
                        column: x => x.CursDarreraActualitacioDadesId,
                        principalTable: "CursosAcademics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Alumnes_Etapes_EtapaActualId",
                        column: x => x.EtapaActualId,
                        principalTable: "Etapes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Actuacions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlumneId = table.Column<int>(type: "INTEGER", nullable: false),
                    TipusActuacioId = table.Column<int>(type: "INTEGER", nullable: false),
                    ObservacionsTipusActuacio = table.Column<string>(type: "TEXT", nullable: false),
                    MomentDeLactuacio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CursActuacioId = table.Column<int>(type: "INTEGER", nullable: false),
                    CentreAlMomentDeLactuacioId = table.Column<int>(type: "INTEGER", nullable: true),
                    EtapaAlMomentDeLactuacioId = table.Column<int>(type: "INTEGER", nullable: true),
                    NivellAlMomentDeLactuacio = table.Column<string>(type: "TEXT", nullable: false),
                    MinutsDuradaActuacio = table.Column<int>(type: "INTEGER", nullable: false),
                    DescripcioActuacio = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actuacions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Actuacions_Alumnes_AlumneId",
                        column: x => x.AlumneId,
                        principalTable: "Alumnes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Actuacions_Centres_CentreAlMomentDeLactuacioId",
                        column: x => x.CentreAlMomentDeLactuacioId,
                        principalTable: "Centres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Actuacions_CursosAcademics_CursActuacioId",
                        column: x => x.CursActuacioId,
                        principalTable: "CursosAcademics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Actuacions_Etapes_EtapaAlMomentDeLactuacioId",
                        column: x => x.EtapaAlMomentDeLactuacioId,
                        principalTable: "Etapes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Actuacions_TipusActuacions_TipusActuacioId",
                        column: x => x.TipusActuacioId,
                        principalTable: "TipusActuacions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Actuacions_AlumneId",
                table: "Actuacions",
                column: "AlumneId");

            migrationBuilder.CreateIndex(
                name: "IX_Actuacions_CentreAlMomentDeLactuacioId",
                table: "Actuacions",
                column: "CentreAlMomentDeLactuacioId");

            migrationBuilder.CreateIndex(
                name: "IX_Actuacions_CursActuacioId",
                table: "Actuacions",
                column: "CursActuacioId");

            migrationBuilder.CreateIndex(
                name: "IX_Actuacions_EtapaAlMomentDeLactuacioId",
                table: "Actuacions",
                column: "EtapaAlMomentDeLactuacioId");

            migrationBuilder.CreateIndex(
                name: "IX_Actuacions_TipusActuacioId",
                table: "Actuacions",
                column: "TipusActuacioId");

            migrationBuilder.CreateIndex(
                name: "IX_Alumnes_CentreActualId",
                table: "Alumnes",
                column: "CentreActualId");

            migrationBuilder.CreateIndex(
                name: "IX_Alumnes_CursDarreraActualitacioDadesId",
                table: "Alumnes",
                column: "CursDarreraActualitacioDadesId");

            migrationBuilder.CreateIndex(
                name: "IX_Alumnes_EtapaActualId",
                table: "Alumnes",
                column: "EtapaActualId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Actuacions");

            migrationBuilder.DropTable(
                name: "Alumnes");

            migrationBuilder.DropTable(
                name: "TipusActuacions");

            migrationBuilder.DropTable(
                name: "Centres");

            migrationBuilder.DropTable(
                name: "CursosAcademics");

            migrationBuilder.DropTable(
                name: "Etapes");
        }
    }
}
