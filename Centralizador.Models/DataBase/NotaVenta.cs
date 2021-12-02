﻿using Centralizador.Models.ApiCEN;

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace Centralizador.Models.DataBase
{
    public class NotaVenta
    {
        /// <summary>
        /// Get 1 F° available of NV.
        /// </summary>
        /// <param name="conexion"></param>
        /// <returns></returns>
        public static async Task<int> GetLastNv(Conexion conexion)
        {
            try
            {
                conexion.Query = "select MAX(NVNumero) + 1  from softland.nw_nventa";
                object result = await Conexion.ExecuteScalarAsync(conexion);
                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
                else
                {
                    return 1; // FOR NEW COMPANIES OR ERROR.
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get count of F° Availables of DTE.
        /// </summary>
        /// <param name="conexion"></param>
        /// <returns></returns>
        public static async Task<int> GetFoliosDisponiblesDTEAsync(Conexion conexion)
        {
            try
            {
                conexion.Query = "EXEC [softland].[DTE_FoliosDisp] @Tipo = N'F', @SubTipo = N'T'";
                DataTable dataTable = await Conexion.ExecuteReaderAsync(conexion);
                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    return dataTable.Rows.Count;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return 0;
        }

        /// <summary>
        /// Delete all NV without DTE asociate.
        /// </summary>
        /// <param name="conexion"></param>
        /// <returns></returns>
        public static async Task<int> DeleteNvAsync(Conexion conexion)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                query.AppendLine("SELECT nv.nvnumero ");
                query.AppendLine("FROM softland.nw_nventa nv ");
                query.AppendLine("INNER JOIN softland.nw_detnv d ");
                query.AppendLine("ON   nv.nvnumero = d.nvnumero ");
                query.AppendLine("LEFT JOIN softland.nw_ffactncrednv() f ");
                query.AppendLine("ON   f.nvnumero = d.nvnumero ");
                query.AppendLine("     AND f.codprod = d.codprod ");
                query.AppendLine("     AND f.nvcorrela = d.nvlinea ");
                query.AppendLine("WHERE f.folio IS NULL ");
                query.AppendLine("ORDER BY nvnumero DESC");
                conexion.Query = query.ToString();
                DataTable dataTable = await Conexion.ExecuteReaderAsync(conexion);
                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    List<string> listQ = new List<string>();
                    foreach (DataRow item in dataTable.Rows)
                    {
                        string q = $"DELETE FROM softland.nw_nventa where NVNumero = {item["nvnumero"]}";
                        listQ.Add(q);
                    }
                    int res = await Conexion.ExecuteNonQueryTranAsync(conexion, listQ);
                    return res; // 1 Success
                }
            }
            catch (Exception)
            {
                return 0;
                throw;
            }
            return 1;
        }

        /// <summary>
        /// Insert a NV & details
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="folioNV"></param>
        /// <param name="codProd"></param>
        /// <param name="conexion"></param>
        /// <returns></returns>
        public static async Task<int> InsertNvAsync(ResultInstruction instruction, int? folioNV, string codProd, Conexion conexion)
        {
            try
            {
                StringBuilder query3 = new StringBuilder();
                StringBuilder query1 = new StringBuilder();
                StringBuilder query2 = new StringBuilder();

                string date;
                string now;
                //if (Environment.MachineName == "DEVELOPER")
                //{
                //    // Developer
                //    now = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                //    date = instruction.PaymentMatrix.PublishDate.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
                //}
                //else
                //{
                now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                date = instruction.PaymentMatrix.PublishDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                //}

                int neto = instruction.Amount;
                double iva = neto * 0.19;
                double total = Math.Ceiling(neto + iva);
                string concepto = $"Concepto: {instruction.AuxiliaryData.PaymentMatrixConcept}";
                string rut;
                if (instruction.ParticipantNew != null)
                {
                    rut = instruction.ParticipantNew.Rut;
                }
                else
                {
                    rut = instruction.ParticipantDebtor.Rut;
                }

                //Cambio de Campo insert NV (para evitar mostrar <Contacto>[BP__][Ago21][L][V01]</Contacto>
                //10-11-2021
                //SolicitadoPor => DespachadoPor
                //sql_insert_Trigger cambio también!!!! ojo!!!! (eliminar o editar los TR ya instalados en la BD)


                string DespachadoPor = instruction.PaymentMatrix.NaturalKey.Remove(0, 4);
                query1.Append("INSERT INTO softland.nw_nventa (CodAux,CveCod,NomCon,nvFeEnt,nvFem,NVNumero,nvObser,VenCod,nvSubTotal, ");
                query1.Append("nvNetoAfecto,nvNetoExento,nvMonto,proceso,nvEquiv,CodMon,nvEstado,FechaHoraCreacion, CodlugarDesp, DespachadoPor) values ( ");
                query1.Append($"'{rut}','1','.','{date}','{date}',{folioNV}, '{concepto}', '1',{neto},{neto},0,{total.ToString(CultureInfo.InvariantCulture)}, ");
                query1.Append($"'Centralizador',1,'01','A','{now}','{instruction.PaymentMatrix.ReferenceCode}', '{DespachadoPor}') ");

                query2.Append("INSERT INTO softland.nw_detnv (NVNumero,nvLinea,nvFecCompr,CodProd,nvCant,nvPrecio,nvSubTotal,nvTotLinea,CodUMed,CantUVta,nvEquiv)VALUES(");
                query2.Append($"{folioNV},1,'{date}','{codProd}',1,{neto},{neto},{neto},'UN',1,1)");

                query3.Append("INSERT INTO softland.NW_Impto (nvNumero, CodImpto, ValPctIni, AfectoImpto, Impto)  VALUES ( ");
                query3.Append($"{folioNV},'IVA',19,{neto},{iva.ToString(CultureInfo.InvariantCulture)})");

                string xx = query2.ToString();
                // Execute Transaction
                if (!string.IsNullOrEmpty(query1.ToString()) || !string.IsNullOrEmpty(query2.ToString()) || !string.IsNullOrEmpty(query3.ToString()))
                {
                    int res = Convert.ToInt32(await Conexion.ExecuteNonQueryTranAsync(conexion, new List<string> { query1.ToString(), query2.ToString(), query3.ToString() }));
                    return res;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}