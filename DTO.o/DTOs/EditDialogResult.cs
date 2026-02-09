using DTO.o.Interfaces;

namespace DTO.o.DTOs
{
    /// <summary>
    /// Resultat d'un diàleg d'edició que pot indicar:
    /// - Edició completada: Data conté el DTO actualitzat
    /// - Element esborrat: DeletedId conté l'Id de l'element esborrat
    /// - Cancel·lat: Tots dos són null
    /// </summary>
    public class EditDialogResult<T> : IDTOo where T : class
    {
        public T? Data { get; }
        public int? DeletedId { get; }
        public bool WasDeleted => DeletedId.HasValue;
        public bool WasUpdated => Data != null;
        public bool WasCancelled => !WasDeleted && !WasUpdated;

        private EditDialogResult(T? data, int? deletedId)
        {
            Data = data;
            DeletedId = deletedId;
        }

        /// <summary>
        /// Crea un resultat indicant que s'ha actualitzat l'element
        /// </summary>
        public static EditDialogResult<T> Updated(T data)
            => new(data, null);

        /// <summary>
        /// Crea un resultat indicant que s'ha esborrat l'element
        /// </summary>
        public static EditDialogResult<T> Deleted(int id)
            => new(null, id);

        /// <summary>
        /// Crea un resultat indicant que s'ha cancel·lat el diàleg
        /// </summary>
        public static EditDialogResult<T> Cancelled()
            => new(null, null);
    }
}
