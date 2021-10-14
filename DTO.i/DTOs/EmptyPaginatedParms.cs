using System;
using DTO.i.Interfaces;

namespace DTO.i.DTOs
{
    public class EmptyPaginatedParms : IDtoi, IPaginated
    {
        public EmptyPaginatedParms(int take, int skip)
        {
            Take = take;
            Skip = skip;
        }

        public int Take { get; }

        public int Skip { get; }
    }
}
