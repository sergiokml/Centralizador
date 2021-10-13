﻿using Centralizador.Models.ApiSII;

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Centralizador.Models.DataBase
{
    public class DteFiles
    {
        #region Properties

        public int ID_Archivo { get; set; }
        public string Extension { get; set; }
        public string Archivo { get; set; }
        public DateTime FechaGenDTE { get; set; }
        public string TipoXML { get; set; }

        #endregion Properties

        /// <summary>
        /// Get Xml from Softland DB (Versión Update Softland 26-11-2020)
        /// </summary>
        /// <param name="conexion"></param>
        /// <param name="nroInt"></param>
        /// <param name="Folio"></param>
        /// <returns></returns>
        public static async Task<List<DteFiles>> GetDteFilesAsync(Conexion conexion, int nroInt, int Folio)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                List<DteFiles> lista = new List<DteFiles>();
                DataTable dataTable;

                query.Append("SELECT TOP 2 ID_Archivo ");
                query.Append("        ,Extension ");
                query.Append("        ,Archivo ");
                query.Append("        ,FechaGenDTE ");
                query.Append("        ,TipoXML ");
                query.Append("FROM softland.DTE_Archivos ");
                query.Append("WHERE Tipo = 'F' ");
                query.Append($"        AND Nroint = {nroInt} ");
                query.Append($"        AND Folio = {Folio} ORDER BY FechaGenDTE DESC  ");
                conexion.Query = query.ToString();
                dataTable = await Conexion.ExecuteReaderAsync(conexion);
                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    foreach (DataRow item in dataTable.Rows)
                    {
                        {
                            DteFiles file = new DteFiles();
                            if (item[0] != DBNull.Value) { file.ID_Archivo = Convert.ToInt32(item[0]); }
                            if (item[1] != DBNull.Value) { file.Extension = Convert.ToString(item[1]); }
                            if (item[2] != DBNull.Value) { file.Archivo = Convert.ToString(item[2]); }
                            if (item[3] != DBNull.Value) { file.FechaGenDTE = Convert.ToDateTime(item[3]); }
                            if (item[4] != DBNull.Value) { file.TipoXML = Convert.ToString(item[4]); }
                            lista.Add(file);
                        }
                    }
                    return lista;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }

        /// <summary>
        /// Get Xml from Softland DB
        /// </summary>
        /// <param name="conexion"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<List<DteFiles>> GetDteFilesAsync(Conexion conexion, int id)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                List<DteFiles> lista = new List<DteFiles>();
                DataTable dataTable;

                query.Append("SELECT ID_Archivo ");
                query.Append("        ,Extension ");
                query.Append("        ,Archivo ");
                query.Append("        ,FechaGenDTE ");
                query.Append("        ,TipoXML ");
                query.Append("FROM softland.DTE_Archivos ");
                query.Append($"WHERE ID_Archivo = {id} ");
                conexion.Query = query.ToString();
                dataTable = await Conexion.ExecuteReaderAsync(conexion);
                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    foreach (DataRow item in dataTable.Rows)
                    {
                        {
                            DteFiles file = new DteFiles();
                            if (item[0] != DBNull.Value) { file.ID_Archivo = Convert.ToInt32(item[0]); }
                            if (item[1] != DBNull.Value) { file.Extension = Convert.ToString(item[1]); }
                            if (item[2] != DBNull.Value) { file.Archivo = Convert.ToString(item[2]); }
                            if (item[3] != DBNull.Value) { file.FechaGenDTE = Convert.ToDateTime(item[3]); }
                            if (item[4] != DBNull.Value) { file.TipoXML = Convert.ToString(item[4]); }
                            lista.Add(file);
                        }
                    }
                    return lista;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="conexion"></param>
        /// <param name="detalle"></param>
        public static async void UpdateFiles(Conexion conexion, Detalle detalle)
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine("UPDATE softland.dte_doccab ");
            query.AppendLine("SET  aceptadocliente = 1 ");
            query.AppendLine("WHERE tipodte = 33 ");
            query.AppendLine($"     AND folio = {detalle.Folio} ");
            query.AppendLine("     AND tipo = 'F' ");
            query.AppendLine($"     AND nroint = {detalle.NroInt} ");
            query.AppendLine($"     AND rutrecep = '{detalle.Instruction.ParticipantDebtor.Rut}-{detalle.Instruction.ParticipantDebtor.VerificationCode}'");
            conexion.Query = query.ToString();
            await Conexion.ExecuteNonQueryAsync(conexion);
        }
    }
}