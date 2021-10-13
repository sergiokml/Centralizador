﻿using Centralizador.Models.ApiSII;

using System.Linq;

namespace Centralizador.Models.Helpers
{
    public class GetReferenceCen
    {
        public DTEDefTypeDocumentoReferencia DocumentoReferencia { get; set; }

        public GetReferenceCen(Detalle detalle)
        {
            DocumentoReferencia = GetRef(detalle);
        }

        private DTEDefTypeDocumentoReferencia GetRef(Detalle detalle)
        {
            try
            {
                DTEDefTypeDocumento obj = (DTEDefTypeDocumento)detalle.DTEDef.Item;
                DTEDefTypeDocumentoReferencia[] refr = obj.Referencia;
                if (refr != null)
                {
                    DTEDefTypeDocumentoReferencia r = refr.FirstOrDefault(x => x.TpoDocRef.ToUpper() == "SEN");
                    if (r != null)
                    {
                        return r;
                    }
                }
            }
            catch (System.Exception)
            {
                return null;
                throw;
            }

            return null;
        }
    }
}