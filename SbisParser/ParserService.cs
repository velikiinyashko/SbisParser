using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SbisParser.Models;
using SbisParser.Interfaces;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace SbisParser
{
    public class ParserService : IHostedService
    {
        private readonly ILogger<ParserService> _logger;
        private readonly SbisSettings _options;
        private readonly IBaseService _service;
        private DateTime _startTime;
        private List<InvoiceModel> _invoices = new();
        private bool _writtenInvoices;
        private bool _writtenInvoiceItems;
        private List<InvoiceItemModel> _invoiceItems = new();
        private XmlReaderSettings settings = new();
        private int _noscf = 0;
        private readonly IHostApplicationLifetime _lifetime;

        public ParserService(ILogger<ParserService> logger, IOptions<SbisSettings> configuration, IHostApplicationLifetime lifetime, IBaseService service)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _logger = logger;
            _options = configuration.Value;
            _service = service;
            _lifetime = lifetime;
            foreach (var file in Directory.GetFiles(_options.SchemesPath))
                settings.Schemas.Add(Path.GetFileName(file), file);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            int currentPosition = 0;
            _startTime = DateTime.Now;
            Console.WriteLine($"Start parser: {_startTime}");
            _logger.LogInformation($"Started find documents in folder: [{_options.DocumentsPath}] \r\n");
            try
            {
                var getfiles = Directory.Exists(_options.DocumentsPath) == true ? Directory.GetFiles(_options.DocumentsPath, _options.MaskFile, SearchOption.AllDirectories) : throw new ArgumentException($"Directory is not found");
                _logger.LogInformation($"Found {getfiles.Length} files");
                foreach (var file in getfiles)
                {
                    parse(file);
                    PercentLog(getfiles.Length, ++currentPosition, _invoices.Count, _invoiceItems.Count, _noscf);
                }

                if (_options.WriteToBase)
                {
                    if (_invoices.Count != 0)
                        _writtenInvoices = await _service.WriteDataToBase(_options.NameTableDataBase.IsCreateTable, _invoices.ToDataTable(_options.NameTableDataBase.invoiceTable));

                    if (_invoiceItems.Count != 0)
                        _writtenInvoiceItems = await _service.WriteDataToBase(_options.NameTableDataBase.IsCreateTable, _invoiceItems.ToDataTable(_options.NameTableDataBase.invoiceItemTable));
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex.Message);
            }
            await StopAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            DateTime _endTime = DateTime.Now;
            _logger.LogInformation($"The parser finished job in {_endTime} | Task execution time: {_endTime - _startTime}");
            _lifetime.StopApplication();
        }

        private void parse(string uri)
        {
            try
            {
                using (XmlReader reader = XmlReader.Create(uri, settings))
                {
                    XmlSerializer serializer = new(typeof(Файл));
                    var obj = (Файл)serializer.Deserialize(reader);
                    if (_options.Function.Contains(obj.Документ.Функция))
                    {
                        if (obj.Документ.СвСчФакт != null)
                        {
                            if (obj.Документ.СвСчФакт.ГрузОт != null)
                            {
                                InvoiceModel invoice = new()
                                {
                                    DateInvoice = obj.Документ.СвСчФакт.ДатаСчФ,
                                    Number = obj.Документ.СвСчФакт.НомерСчФ,
                                    INNOrg = obj.Документ.СвСчФакт.ГрузПолуч != null ?
                                        obj.Документ.СвСчФакт.ГрузПолуч.ИдСв.СвЮЛУч.ИННЮЛ.ToString() :
                                        null,
                                    KPPOrg = obj.Документ.СвСчФакт.ГрузПолуч != null ?
                                        obj.Документ.СвСчФакт.ГрузПолуч.ИдСв.СвЮЛУч.КПП != null ?
                                        obj.Документ.СвСчФакт.ГрузПолуч.ИдСв.СвЮЛУч.КПП.ToString() :
                                        null :
                                        null,
                                    INNSupplier = obj.Документ.СвСчФакт.ГрузОт.ГрузОтпр != null ?
                                        obj.Документ.СвСчФакт.ГрузОт.ГрузОтпр.ИдСв.СвЮЛУч.ИННЮЛ.ToString() :
                                        obj.Документ.СвСчФакт.СвПрод.ИдСв.СвЮЛУч.ИННЮЛ.ToString(),
                                    KPPSupplier = obj.Документ.СвСчФакт.ГрузОт.ГрузОтпр != null ?
                                        obj.Документ.СвСчФакт.ГрузОт.ГрузОтпр.ИдСв.СвЮЛУч.КПП != null ?
                                        obj.Документ.СвСчФакт.ГрузОт.ГрузОтпр.ИдСв.СвЮЛУч.КПП.ToString() :
                                        obj.Документ.СвСчФакт.СвПрод.ИдСв.СвЮЛУч.КПП.ToString() :
                                        null,
                                    NumDogovor = obj.Документ.СвПродПер != null ?
                                        obj.Документ.СвПродПер.СвПер.ОснПер.НомОсн :
                                        null,
                                    SumExtVat = obj.Документ.ТаблСчФакт.ВсегоОпл.СтТовБезНДСВсего,
                                    SumIncVat = obj.Документ.ТаблСчФакт.ВсегоОпл.СтТовУчНалВсего,
                                };
                                _invoices.Add(invoice);

                                foreach (var item in obj.Документ.ТаблСчФакт.СведТов)
                                {
                                    InvoiceItemModel invoiceItem = new()
                                    {
                                        Invoice = obj.Документ.СвСчФакт.НомерСчФ,
                                        Title = item.НаимТов,
                                        CountItem = item.КолТов,
                                        PriceExtVat = item.СтТовБезНДС,
                                        PriceIncVat = item.СтТовУчНал,
                                        Price = item.ЦенаТов,
                                        VAT = item.НалСт != "без НДС" ? int.Parse(item.НалСт.Trim('%')) : 0,
                                        VATSum = item.СумНал.СумНал,
                                        DateInvoice = invoice.DateInvoice,
                                        INNOrg = invoice.INNOrg,
                                        KPPOrg = invoice.KPPOrg,
                                        INNSupplier = invoice.INNSupplier,
                                        KPPSupplier = invoice.KPPSupplier 
                                    };
                                    _invoiceItems.Add(invoiceItem);
                                }
                            }
                            else if (obj.Документ.СвСчФакт.СвПрод != null)
                            {
                                InvoiceModel invoice = new()
                                {
                                    DateInvoice = obj.Документ.СвСчФакт.ДатаСчФ,
                                    Number = obj.Документ.СвСчФакт.НомерСчФ,
                                    INNOrg = obj.Документ.СвСчФакт.СвПокуп != null ? obj.Документ.СвСчФакт.СвПокуп.ИдСв != null ? obj.Документ.СвСчФакт.СвПокуп.ИдСв.СвЮЛУч.ИННЮЛ.ToString() : null : null,
                                    KPPOrg = obj.Документ.СвСчФакт.СвПокуп != null ? obj.Документ.СвСчФакт.СвПокуп.ИдСв != null ? obj.Документ.СвСчФакт.СвПокуп.ИдСв.СвЮЛУч.КПП.ToString() : null : null,
                                    INNSupplier = obj.Документ.СвСчФакт.СвПрод.ИдСв != null ? obj.Документ.СвСчФакт.СвПрод.ИдСв.СвЮЛУч.ИННЮЛ.ToString() : null,
                                    KPPSupplier = obj.Документ.СвСчФакт.СвПрод.ИдСв != null ? obj.Документ.СвСчФакт.СвПрод.ИдСв.СвЮЛУч.КПП.ToString() : null,
                                    NumDogovor = obj.Документ.СвПродПер != null ? obj.Документ.СвПродПер.СвПер.ОснПер.НомОсн : null,
                                    SumExtVat = obj.Документ.ТаблСчФакт.ВсегоОпл.СтТовБезНДСВсего,
                                    SumIncVat = obj.Документ.ТаблСчФакт.ВсегоОпл.СтТовУчНалВсего,
                                };
                                _invoices.Add(invoice);

                                foreach (var item in obj.Документ.ТаблСчФакт.СведТов)
                                {
                                    InvoiceItemModel invoiceItem = new()
                                    {
                                        Invoice = obj.Документ.СвСчФакт.НомерСчФ,
                                        Title = item.НаимТов,
                                        CountItem = item.КолТов,
                                        PriceExtVat = item.СтТовБезНДС,
                                        PriceIncVat = item.СтТовУчНал,
                                        Price = item.ЦенаТов,
                                        VAT = item.НалСт != "без НДС" ? item.НалСт.Contains("%") ? int.Parse(item.НалСт.Trim('%')) : int.Parse(item.НалСт.Split('/')[0]) : 0,
                                        VATSum = item.СумНал.СумНал
                                    };
                                    _invoiceItems.Add(invoiceItem);
                                }
                            }
                            else
                            {
                                try
                                {
                                    ++_noscf;
                                    reader.Close();
                                    uri.MoveFile(_options.IsIncorectDocumentsPath);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"File error: {uri}\r\n Error: {ex.Message}\r\n");
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                ++_noscf;
                                reader.Close();
                                uri.MoveFile(_options.IsIncorectDocumentsPath);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"File error: {uri}\r\n Error: {ex.Message}\r\n");
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            ++_noscf;
                            reader.Close();
                            uri.MoveFile(_options.IsIncorectDocumentsPath);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"File error: {uri}\r\n Error: {ex.Message}\r\n");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                ++_noscf;
                _logger.LogError($"File error: {uri}\r\n Error: {ex.Message}\r\n");
                uri.MoveFile(_options.IsIncorectDocumentsPath);
            }
        }

        public void PercentLog(int count, int position, int invoicesCount, int invoicesItems, int noparse)
        {
            double pr = Math.Round((double)(position * 100 / count));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.WriteLine($"Percent: [{pr}%] | Count: [{position} from {count}]|[invoice:{invoicesCount}|items:{invoicesItems}]|[Move:{noparse}]");
            Console.ResetColor();
        }
    }
}
