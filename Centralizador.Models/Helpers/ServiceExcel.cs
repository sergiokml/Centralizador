﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Centralizador.Models.ApiCEN;
using Centralizador.Models.ApiSII;
using Centralizador.Models.DataBase;

using Spire.Xls;

namespace Centralizador.Models.Helpers
{
    public class ServiceExcel
    {
        private List<Detalle> Detalles { get; set; }
        private ResultParticipant UserParticipant { get; set; }
        private string TokenCen { get; set; }

        public ServiceExcel(ResultParticipant userParticipant)
        {
            UserParticipant = userParticipant;
        }

        public ServiceExcel(List<Detalle> detalles, ResultParticipant userParticipant, string tokenCen)
        {
            Detalles = detalles;
            UserParticipant = userParticipant;
            TokenCen = tokenCen;
        }

        public void CreateNomina(BackgroundWorker BgwPay)
        {
            BgwPay.DoWork += BgwPay_DoWork;
            BgwPay.RunWorkerAsync();
        }

        public void ExportToExcel(List<Detalle> detalles, bool isCreditor, object month)
        {
            int c = 0;
            DataTable table = new DataTable();
            table.Columns.Add("N°");
            table.Columns.Add("Emission");
            table.Columns.Add("Name");
            table.Columns.Add("Rut");
            DataColumn colInt32 = new DataColumn("F°")
            {
                DataType = Type.GetType("System.Int32")
            };
            table.Columns.Add(colInt32);


            //table.Columns.Add("F°", Type.GetType("System.Int32"),"F°");
            table.Columns.Add("Glosa");
            table.Columns.Add("Net");
            table.Columns.Add("Tax 19%");
            table.Columns.Add("Exent");
            table.Columns.Add("Total");
            table.Columns.Add("Instruction Id");
            table.Columns.Add("Detail");
            table.Columns.Add("Sending Date");
            table.Columns.Add("Status");
            table.Columns.Add("NC");
            table.Columns.Add("FolioRef");
            table.Columns.Add("Publication");

            foreach (Detalle item in detalles)
            {
                //if (item.MntNeto <= 9)
                //{
                //    continue;
                //}
                DataRow row = table.NewRow();
                c++;
                row["N°"] = c;
                if (item.Folio > 0)
                {
                    row["F°"] = item.Folio;
                    //row[2] = string.Format(CultureInfo.InvariantCulture, "{0:d-MM-yyyy}", Convert.ToDateTime(item.FechaEmision));  = 4-03-2021
                    row["Emission"] = Convert.ToDateTime(item.FechaEmision).ToString("dd/MM/yyyy"); // = 04/03/2021
                }

                row["Rut"] = item.RutReceptor + "-" + item.DvReceptor;
                row["Name"] = item.RznSocRecep;
                if (item.Instruction != null)
                {
                    row["Instruction Id"] = item.Instruction.Id;
                }
                row["Net"] = item.MntNeto;
                row["Exent"] = item.MntExento;
                row["Tax 19%"] = item.MntIva;
                row["Total"] = item.MntTotal;
                if (item.DTEDef != null)
                {
                    DTEDefTypeDocumento dte = (DTEDefTypeDocumento)item.DTEDef.Item;
                    DTEDefTypeDocumentoDetalle[] details = dte.Detalle;
                    row[10] = details[0].NmbItem.ToLowerInvariant();
                }
                row[11] = item.FechaRecepcion;
                row[12] = item.StatusDetalle;
                if (item.DataEvento != null && item.DataEvento.ListEvenHistDoc != null)
                {
                    if (item.DataEvento.ListEvenHistDoc.Count > 0)
                    {
                        if (item.DataEvento.ListEvenHistDoc.FirstOrDefault(x => x.CodEvento == "NCA") != null) // // Recepción de NC de anulación que referencia al documento.
                        {
                            row[13] = "NCA";
                        }
                    }
                }
                if (item.Instruction != null && item.Instruction.PaymentMatrix != null)
                {
                    row[14] = item.Instruction.PaymentMatrix.NaturalKey;
                    row[15] = item.Instruction.PaymentMatrix.ReferenceCode;
                    row[16] = item.Instruction.PaymentMatrix.PublishDate.ToString("dd-MM-yyyy");
                }
                table.Rows.Add(row);
                // NEW ROW FOR HISTORY.
                if (item.DteInfoRefs != null && item.DteInfoRefs.Count > 1)
                {
                    foreach (DteInfoRef r in item.DteInfoRefs.OrderBy(x => x.Folio))
                    {
                        if (r.Folio != item.Folio && item.Instruction.PaymentMatrix.NaturalKey == r.Glosa && item.Instruction.PaymentMatrix.ReferenceCode == r.FolioRef)
                        {
                            row = table.NewRow();
                            row[1] = r.Folio;
                            row[2] = r.AuxDocfec.ToString("dd-MM-yyyy");
                            if (r.AuxDocNum > 0) { row[13] = r.AuxDocNum; }
                            row[6] = r.NetoAfecto * -1;
                            table.Rows.Add(row);
                        }
                    }
                }
            }
            // Save Excel
            if (table.Rows.Count > 0)
            {
                string nameFile;
                Workbook workbook = new Workbook();
                Worksheet worksheet = workbook.Worksheets[0];
                worksheet.InsertDataTable(table, true, 1, 1);
                if (isCreditor)
                {
                    nameFile = $"{UserParticipant.Name}_Creditor{month.ToString().ToUpper()}_{DateTime.Now:dd-MM-yyyy-HH-mm-ss}" + ".xlsx";
                }
                else
                {
                    nameFile = $"{UserParticipant.Name}_Debtor{month.ToString().ToUpper()}_{DateTime.Now:dd-MM-yyyy-HH-mm-ss}" + ".xlsx";
                }

                string path = Path.GetTempPath() + @"Log\";
                new CreateFile(path);
                workbook.SaveToFile(path + nameFile, FileFormat.Version2016);
                ProcessStartInfo process = new ProcessStartInfo(path + nameFile)
                {
                    WindowStyle = ProcessWindowStyle.Normal
                };
                Process.Start(process);
            }
        }

        private void BgwPay_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker BgwPay = sender as BackgroundWorker;
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            int c = 0;
            // https://ww4.indexa.cl/foindexaabonos/fotefbm/AyudaExcel.asp

            DataTable table = new DataTable();
            table.Columns.Add("1"); //2
            table.Columns.Add("2"); //rut sin guión
            table.Columns.Add("3"); //razón social
            table.Columns.Add("4"); //nro cta
            table.Columns.Add("5"); //monto
            table.Columns.Add("6"); //1:cta cte - 2:vista - 3:ahorro
            table.Columns.Add("7"); //código banco
            table.Columns.Add("8");  //email

            foreach (Detalle item in Detalles)
            {
                try
                {
                    // Security Bank Txt
                    DataRow row = table.NewRow();
                    row["1"] = "2";
                    row["2"] = string.Concat(item.RutReceptor, item.DvReceptor); // Rut sin guión
                    row["3"] = ti.ToTitleCase(item.RznSocRecep.ToLower());  // Rzn Social
                    if (item.Instruction != null && item.Instruction.ParticipantCreditor != null)
                    {
                        row["4"] = item.Instruction.ParticipantCreditor.BankAccount.TrimStart(new char[] { '0' }).Replace("-", ""); // Nro
                    }
                    row["5"] = item.MntTotal; // Amount
                    row["6"] = "1";
                    if (item.Instruction != null && item.Instruction.ParticipantCreditor != null)
                    {
                        switch (item.Instruction.ParticipantCreditor.Bank)
                        {
                            case 1: // Chile
                                row["7"] = "1";
                                break;

                            case 2: // International
                                row["7"] = "9";
                                break;

                            case 3: // Scotiabank
                                row["7"] = "14";
                                break;

                            case 4: // Bci
                                row["7"] = "16";
                                break;

                            case 5: // Bice
                                row["7"] = "28";
                                break;

                            case 6: // Hsbc
                                row["7"] = "31";
                                break;

                            case 7: // Santander
                                row["7"] = "37";
                                break;

                            case 8: // Itau/CorpBanca
                                row["7"] = "39"; // Itau
                                break;

                            case 9: // Security
                                row["7"] = "49";
                                break;

                            case 10: // Falabella
                                row["7"] = "51";
                                break;

                            case 11: // Ripley
                                row["7"] = "53";
                                break;

                            case 12: // Rabobank
                                row["7"] = "54";
                                break;

                            case 13: // Concorcio
                                row["7"] = "55";
                                break;

                            case 16: // Bbva
                                row["7"] = "504";
                                break;

                            case 18: // Estado
                                row["7"] = "12";
                                break;

                            default:
                                row["7"] = "";
                                break;
                        }

                        if (item.Instruction != null && item.Instruction.ParticipantCreditor.BillsContact != null && item.Instruction.ParticipantCreditor.BillsContact.Email != null)
                        {
                            row["8"] = item.Instruction.ParticipantCreditor.BillsContact.Email;
                        }
                        else
                        {
                            if (item.Instruction != null && item.Instruction.ParticipantCreditor.BillsContact != null && item.Instruction.ParticipantCreditor.PaymentsContact.Email != null)
                            {
                                row["8"] = item.Instruction.ParticipantCreditor.PaymentsContact.Email;
                            }
                        }
                    }
                    table.Rows.Add(row);
                    // Insert pay to CEN
                    if (item.Instruction != null && item.Instruction.Dte != null && !item.Instruction.IsPaid)
                    {
                        ResultPay pay = new ResultPay
                        {
                            ActualCollector = "",
                            Amount = item.Instruction.Amount,
                            Creditor = item.Instruction.Creditor,
                            Debtor = item.Instruction.Debtor,
                            InstructionAmountTuples = new List<List<int>>() { new List<int>() { item.Instruction.Id, item.MntTotal } },
                            PaymentDt = string.Format("{0:yyyy-MM-dd}", DateTime.Now),
                            Dtes = new List<long>() { item.Instruction.Dte.Id },
                            TransactionType = 3
                        };
                        ResultPay resultPay = Pay.SendPayAsync(pay, TokenCen).Result;
                        if (resultPay == null)
                        {
                            // Error Exception
                            MessageBox.Show("There was an error connecting to the CEN API.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        item.Instruction.StatusPaid = Pay.StatusPay.Pagado;
                        item.Instruction.IsPaid = true;
                    }
                    // Insert Softland
                }
                catch (Exception)
                {
                    throw;
                }
                c++;
                float porcent = (float)(100 * c) / Detalles.Count;
                BgwPay.ReportProgress((int)porcent, $"Generating file, please wait. ({c}/{Detalles.Count})");
            }

            // Save Excel
            if (table.Rows.Count > 0)
            {
                Workbook workbook = new Workbook();
                Worksheet worksheet = workbook.Worksheets[0];
                worksheet.InsertDataTable(table, false, 1, 1);
                string nameFile = $"{UserParticipant.Name}_NominaBank_{DateTime.Now:dd-MM-yyyy-HH-mm-ss}" + ".xlsx";
                string path = Path.GetTempPath() + @"Log\";
                new CreateFile(path);
                workbook.SaveToFile(path + nameFile, FileFormat.Version2016);
                ProcessStartInfo process = new ProcessStartInfo(path + nameFile)
                {
                    WindowStyle = ProcessWindowStyle.Minimized
                };
                Process.Start(process);
                e.Result = table;
            }
        }
    }
}