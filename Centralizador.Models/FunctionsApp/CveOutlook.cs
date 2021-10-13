﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using Centralizador.Models.ApiCEN;
using Centralizador.Models.ApiSII;
using Centralizador.Models.DataBase;
using Centralizador.Models.Helpers;
using Centralizador.Models.registroreclamodteservice;

using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;

using MimeKit;

using static Centralizador.Models.Helpers.HEnum;

namespace Centralizador.Models.FunctionsApp
{
    public class CveOutlook : ICveFunction
    {
        public CancellationTokenSource Cancellation { get; set; }
        public string DBName { get; set; }
        public HPgModel PgModel { get; set; }
        public IProgress<HPgModel> Progress { get; set; }
        public string Query { get; set; }
        public string TokenCen { get; set; }
        public string TokenSii { get; set; }
        public ResultParticipant UserParticipant { get; set; }
        public List<Detalle> DetalleList { get; set; }
        public Conexion Conn { get; set; }
        public TipoTask Mode { get; set; }
        public List<ResultBilingType> BilingTypes { get; set; }
        public StringBuilder StringLogging { get; set; }
        public bool IsRuning { get; set; }

        public CveOutlook(string tokenSii, IProgress<HPgModel> progress)
        {
            TokenSii = tokenSii;
            Progress = progress;
            PgModel = new HPgModel();
            Progress = progress;
            IsRuning = true;
        }

        public CveOutlook()
        {
        }



        private void SaveXmlInFolder(string nameFolder, string nameFile, DTEDefType dte)
        {
            try
            {
                string path = Properties.Settings.Default.IPServer + @"Inbox\" + nameFolder;
                new CreateFile(path);
                File.WriteAllText(path + @"\" + nameFile + ".xml", HSerialize.DTE_To_Xml(dte));
            }
            catch (Exception)
            {
                throw;
            }
        }

        private int SaveFiles(XDocument xDocument)
        {
            string response = "";
            string nameFolder;
            string nameFile;
            // DESERIALIZE.
            EnvioDTE xmlObjeto = HSerialize.ENVIODTE_To_Object(xDocument);
            if (xmlObjeto != null)
            {
                foreach (DTEDefType dte in xmlObjeto.SetDTE.DTE)
                {
                    DTEDefTypeDocumento document = (DTEDefTypeDocumento)dte.Item;
                    string[] emisor = document.Encabezado.Emisor.RUTEmisor.Split('-');
                    if (document.Encabezado.IdDoc.TipoDTE == DTEType.Item33 || document.Encabezado.IdDoc.TipoDTE == DTEType.Item34)
                    {
                        document.Encabezado.IdDoc.Folio = document.Encabezado.IdDoc.Folio.TrimStart(new char[] { '0' }); // REMOVE ZERO LEFT.
                        try
                        {
                            using (RegistroReclamoDteServiceEndpointService dateTimeDte = new RegistroReclamoDteServiceEndpointService(TokenSii))
                            {
                                int tipo = Convert.ToInt32(document.Encabezado.IdDoc.TipoDTE);
                                response = dateTimeDte.consultarFechaRecepcionSii(emisor.GetValue(0).ToString(),
                                emisor.GetValue(1).ToString(),
                                tipo.ToString(),
                                document.Encabezado.IdDoc.Folio);
                            }

                            if (response.Length != 0)
                            {
                                DateTime timeResponse = DateTime.Parse(string.Format(CultureInfo.InvariantCulture, "{0:D}", response));
                                // 2020\06\76470581-5
                                nameFolder = timeResponse.Year + @"\" + timeResponse.Month + @"\" + document.Encabezado.Receptor.RUTRecep;
                                // 15357870_33_8888
                                nameFile = document.Encabezado.Emisor.RUTEmisor + "__" + Convert.ToInt32(document.Encabezado.IdDoc.TipoDTE).ToString() + "__" + document.Encabezado.IdDoc.Folio;
                                SaveXmlInFolder(nameFolder, nameFile, dte);
                            }
                            else
                            {
                                // Errors. // dejar carpeta folder en 2020/5
                                nameFolder = document.Encabezado.IdDoc.FchEmis.Year + @"\" + document.Encabezado.IdDoc.FchEmis.Month;
                                nameFile = document.Encabezado.Emisor.RUTEmisor + "__" + Convert.ToInt32(document.Encabezado.IdDoc.TipoDTE).ToString() + "__" + document.Encabezado.IdDoc.Folio;
                                SaveXmlInFolder(nameFolder + @"\Errors\", nameFile, dte);
                            }
                        }
                        catch (Exception)
                        {
                            // ERROR.
                            return 1;
                        }
                    }
                }
            }
            // SUCCESS.
            return 0;
        }

        private IEnumerable<MimeEntity> GetXmlAttachments(IEnumerable<MimeEntity> bodyParts)
        {
            foreach (MimeEntity bodyPart in bodyParts)
            {
                MessagePart rfc822 = bodyPart as MessagePart;

                if (rfc822 != null)
                {
                    foreach (MimeEntity attachment in GetXmlAttachments(rfc822.Message.BodyParts))
                    {
                        yield return attachment;
                    }
                }
                else
                {
                    string fileName = bodyPart.ContentDisposition?.FileName;
                    if (fileName == null)
                    {
                        fileName = ((MimePart)bodyPart).FileName;
                    }

                    if (fileName != null && fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        yield return bodyPart;
                    }
                }
            }
        }

        public void SaveLogging(string path, string nameFile)
        {
            new CreateFile(path, StringLogging, nameFile);
        }

        public DateTime GetLastDateTime()
        {
            return Properties.Settings.Default.DateTimeEmail;
        }

        public Task ReportProgress(float p, string msg)
        {
            return Task.Run(() =>
            {
                PgModel.PercentageComplete = (int)p;
                PgModel.Msg = msg;
                Progress.Report(PgModel);
            });
        }

        public Task CancelTask()
        {
            return Task.Run(() =>
            {
                Cancellation.Cancel();
            });
        }

        public async Task GetDocFromStore(DateTime period)
        {
            // INFO
            PgModel.StopWatch.Start();

            using (ImapClient client = new ImapClient())
            {
                Cancellation = new CancellationTokenSource();
                int c = 0;
                try
                {
                    await client.ConnectAsync("outlook.office365.com", 993, true);
                    await ReportProgress(0, $"Authenticating on mail server... Please wait.");
                    await client.AuthenticateAsync(Properties.Settings.Default.User365, Properties.Settings.Default.Password365);

                    IList<IMailFolder> folders = await client.GetFoldersAsync(client.PersonalNamespaces[0]);
                    foreach (IMailFolder f in folders)
                    {
                        var x = f.FullName;
                        if (f.Name == "INBOX" || f.Name == "Correo no deseado" || f.Name == "Junk Email Vade Secure")
                        {
                            await f.OpenAsync(FolderAccess.ReadOnly);
                            UniqueIdRange range = new UniqueIdRange(new UniqueId(Convert.ToUInt32(Properties.Settings.Default.UIDRange)), UniqueId.MaxValue);
                            BinarySearchQuery query = SearchQuery.DeliveredAfter(Properties.Settings.Default.DateTimeEmail).And(SearchQuery.ToContains(Properties.Settings.Default.User365));
                            IList<UniqueId> listM = await f.SearchAsync(range, query, Cancellation.Token);
                            int total = listM.Count;
                            foreach (UniqueId uid in await f.SearchAsync(range, query, Cancellation.Token))
                            {
                                MimeMessage message = await f.GetMessageAsync(uid, Cancellation.Token);
                                if (message.Attachments != null)
                                {
                                    IEnumerable<MimeEntity> attachments = GetXmlAttachments(message.BodyParts);
                                    foreach (MimeEntity item in attachments)
                                    {
                                        using (MemoryStream memory = new MemoryStream())
                                        {
                                            if (item is MessagePart rfc822)
                                            {
                                                await rfc822.Message.WriteToAsync(memory, Cancellation.Token);
                                            }
                                            else
                                            {
                                                MimePart part = (MimePart)item;
                                                await part.Content.DecodeToAsync(memory, Cancellation.Token);
                                            }
                                            memory.Position = 0;
                                            XDocument xDocument = null;
                                            try
                                            {
                                                xDocument = XDocument.Load(memory);
                                            }
                                            catch (XmlException)
                                            {
                                                //throw;
                                            }
                                            if (xDocument != null)
                                            {
                                                if (xDocument.Root.Name.LocalName == "EnvioDTE")
                                                //! sería bueno aceptar doc con el root: "DTE" ?? (30-07-2021)
                                                {
                                                    int res = SaveFiles(xDocument);
                                                }
                                            }

                                        }
                                    }
                                    float porcent = (float)(100 * c) / total;
                                    await ReportProgress(porcent, $"Dowloading messages...: {f.FullName}");
                                }
                                if (f == folders[1])
                                {
                                    Properties.Settings.Default.DateTimeEmail = message.Date.DateTime;
                                    Properties.Settings.Default.UIDRange = uid.ToString();
                                    c++;
                                    float porcent = (float)(100 * c) / total;
                                    PgModel.FchOutlook = Properties.Settings.Default.DateTimeEmail;
                                    //SaveParam();
                                    await ReportProgress(porcent, $"Dowloading messages... [{string.Format(CultureInfo.CreateSpecificCulture("es-CL"), "{0:d-MM-yyyy HH:mm}", message.Date.DateTime)}] ({c}/{total})  Subject: {message.Subject} ");
                                }
                            }
                        }
                    }
                }
                catch (Exception) when (Cancellation.Token.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    Properties.Settings.Default.Save();
                    await client.DisconnectAsync(true);
                }
            }
        }

        public void Dispose()
        {
            // INFO
            PgModel.StopWatch.Stop();
            IsRuning = false;
            Properties.Settings.Default.Save();
            Cancellation.Dispose();
        }

        public Task ConvertXmlToPdf(TipoTask task, List<Detalle> lista)
        {
            throw new NotImplementedException();
        }

        public Task ConvertXmlToPdf(Detalle d, TipoTask task)
        {
            throw new NotImplementedException();
        }

        public Task GetDocFromStoreAnual(int period)
        {
            throw new NotImplementedException();
        }
    }
}