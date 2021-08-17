using System;
using DTO.i.Interfaces;

namespace DTO.i.DTOs
{
    public class AlumneSearchParms : IDtoi, IPaginated
    {
        public AlumneSearchParms(
            int take = 1000,
            int skip = 0,
            int? centreId = null,
            DateTime? ambDarreraModificacioAnteriorA = null,
            DateTime? ambDarreraModificacioPosteriorA = null,
            DateTime? ambDataNaixementAnteriorA = null,
            DateTime? ambDataNaixementPosteriorA = null,
            DateTime? ambDataInformeAnteriorA = null,
            DateTime? ambDataInformePosteriorA = null,
            int? etapaId = null,
            int? tipusActuacioId = null,
            string nomCognoms = "",
            string tags = "",
            bool? esActiu = null,
            OrdreResultatsChoice ordreResultats = OrdreResultatsChoice.DarreraModificacio)
        {
            Take = take;
            Skip = skip;
            CentreId = centreId;
            AmbDarreraModificacioAnteriorA = ambDarreraModificacioAnteriorA;
            AmbDarreraModificacioPosteriorA = ambDarreraModificacioPosteriorA;
            AmbDataNaixementAnteriorA = ambDataNaixementAnteriorA;
            AmbDataNaixementPosteriorA = ambDataNaixementPosteriorA;
            AmbDataInformeAnteriorA = ambDataInformeAnteriorA;
            AmbDataInformePosteriorA = ambDataInformePosteriorA;
            EtapaId = etapaId;
            TipusActuacioId = tipusActuacioId;
            NomCognoms = nomCognoms;
            Tags = tags;
            EsActiu = esActiu;
            OrdreResultats = ordreResultats;
        }

        public enum OrdreResultatsChoice
        {
            CognomsNom,
            DarreraModificacio,
        }

        public int Take {get;}
        public int Skip {get;}

        // Centre
        public int? CentreId {get;}

        // Data Actuació
        public DateTime? AmbDarreraModificacioAnteriorA {get;}
        public DateTime? AmbDarreraModificacioPosteriorA {get;}

        // Data Naixement
        public DateTime? AmbDataNaixementAnteriorA {get;}
        public DateTime? AmbDataNaixementPosteriorA {get;}

        // Data Informe
        public DateTime? AmbDataInformeAnteriorA {get;}
        public DateTime? AmbDataInformePosteriorA {get;}

        // Etapa
        public int? EtapaId {get;}

        // Tipus Actuació
        public int? TipusActuacioId {get;}

        // Nom - Cognoms
        public string NomCognoms {get;}

        // Tags
        public string Tags {get;}

        // Actiu
        public bool? EsActiu {get; }

        // Ordre
        public OrdreResultatsChoice OrdreResultats {get;}



    }
}
