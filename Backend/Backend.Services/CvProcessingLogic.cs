using System.Threading.Tasks;
using AI.Dev.OpenAI.GPT;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Backend.Common.Models;
using Backend.Common.Interfaces;
using Backend.Common.Interfaces.DataAccess;
using Backend.Models;
using Backend.Common.Logic;
using System.Linq;
using System.IO;
using Microsoft.Extensions.Configuration;
using Azure.AI.OpenAI;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Reflection;
using System.Text.Json;
using System.Text;


namespace Backend.Service.BusinessLogic
{
    public readonly record struct Section(
        string Id,
        string Content,
        string Filename,
        string Page,
        string FileUri,
        string Title,
        string Folder,
        string[] GroupIds,
        string[] Tags,
        string Category,
        int PageNumber);
    public readonly record struct PageDetail(
    int Index,
    int Offset,
    string Text);

    /// </inheritdoc/>
    public partial class CvProcessingLogic : BaseLogic, ICvsProcessingLogic
    {
        [GeneratedRegex("[^0-9a-zA-Z_-]")]
        private static partial Regex matchInSetRegex();
        private readonly string azureOpenAIApiEndpoint;
        private readonly string AzureOpenAIApiKey;
        private readonly string azureDocumentInteligenceApiEndpoint;
        private readonly string AzureDocumentInteligenceApiKey;
        private readonly string azureOpenAIEmbeddingModel;
        private readonly string azureOpenAIChatCompletionModel;
        private readonly string maxTokens;
        private string storageConnectionString;
        private OpenAIClient openAIClient; 
        private DocumentAnalysisClient documentAnalysisClient;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Gets by DI the dependeciees
        /// </summary>
        /// <param name="sessionProvider"></param>
        /// <param name="dataAccess"></param>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        public CvProcessingLogic(ISessionProvider sessionProvider, IDataAccess dataAccess, IConfiguration configuration, ILogger<ICvsProcessingLogic> logger) : base(sessionProvider, dataAccess, logger)
        {
            this.azureOpenAIApiEndpoint = configuration["AzureOpenAIApiEndpoint"];
            this.AzureOpenAIApiKey = configuration["AzureOpenAIApiKey"];
            this.azureOpenAIEmbeddingModel = configuration["AzureOpenAIEmbeddingModel"];
            this.azureOpenAIChatCompletionModel = configuration["AzureOpenAIChatCompletionModel"];
            this.storageConnectionString = configuration["StorageConnectionString"];
            this.maxTokens = configuration["MaxTokens"];
            this.azureDocumentInteligenceApiEndpoint = configuration["AzureDocumentInteligenceApiEndpoint"];
            this.AzureDocumentInteligenceApiKey = configuration["AzureDocumentInteligenceApiKey"];
            _configuration = configuration;

        }



        /// <inheritdoc/>      
        public async Task<Result<List<ReceivedCv>>> GetReceivedCvsHistoryAsync(int page)
        {
            try
            {
                var token = this.dataAccess.GetSasToken("cv-procesados", 60);
                var data = (await this.dataAccess.ReceivedCvs.GetAsync()).ToList();
                foreach (var item in data)
                {
                    item.FileUri = $"{item.FileUri}?{token}";
                }
                return new Result<List<ReceivedCv>>(data, true,  string.Empty);
            }
            catch (Exception ex)
            {
               return Error<List<ReceivedCv>>(ex);
            }
        }


        /// <summary>
        /// extrae la informacion de un Cv usando doc inteligence y lo califica usando OpenAI.
        /// </summary>
        
        public async Task<bool> ProcessCvFromStorageAsync(Stream file, string filename)
        {
            try
            {
                this.logger.LogInformation($"Processing Cv from storage: {filename}");

                
                string name = filename;

                
                // Obtener la fecha 
                var ProcessingDate = DateTime.Now;
                var date = DateTime.Now.ToString("yyyyMMddHHmmss");

                string cvNoProceced = "cv-sin-procesar";
                string cvProceced = "cv-procesados";




                DataAccess.DataAccess dataAccess = new DataAccess.DataAccess(_configuration);

                string uriCV = await dataAccess.CopyBlobAsync(filename, name, cvNoProceced, cvProceced);

                this.logger.LogInformation($"Se ha movido el archivo del CV al contenedor {cvProceced} y se ha eliminado del contenedor: {cvNoProceced}");


                var sasToken = this.dataAccess.GetSasToken(cvProceced, 60);

                var blobUrl = $"{uriCV}?{sasToken}";


                this.logger.LogInformation($"La uri del Cv es: {blobUrl}");

                this.logger.LogInformation($"Extractayendo texto del Cv utilizando Doc Inteligence");


                var pdfText = await ExtractTextFromPdfAsync(cvProceced, name, blobUrl);


                StringBuilder sb = new StringBuilder(); // StringBuilder para concatenar el contenido

                foreach (var section in pdfText)
                {
                    var processPdfFile = section.Content; // Acceder al contenido de la sección actual
                    sb.AppendLine(processPdfFile); // Agregar el contenido a StringBuilder, con salto de línea entre secciones
                }

                string allContent = sb.ToString();

                this.logger.LogInformation($"Informacion extraida por Doc Inteligence: \n{allContent}");

                string jobInformation = @"Ingeniero/a de Datos Senior. \n Funciones: \r\n\r\nElaborar propuestas de mejora en las soluciones de data analytics existentes, desde el frente de ingeniería,
                para lograr eficiencia operativa en la arquitectura existente de GCP. Diseñar, desarrollar, probar y desplegar procesos de extracción, transformación y carga (ETL y ELT)
                de datos estructurados y no estructurados. \r\n\r\nImplementar soluciones de ingeniería de datos que aseguren el cumplir con los objetivos de los proyectos de Centria y Clientes. 
                Velar porque todos los desarrollos (propios y realizados en conjunto con partners/proveedores) \r\n\r\nMonitorear y asegurar un óptimo performance de las soluciones end to end,
                a fin de mitigar incidencias en las soluciones existentes. Plantear un Dashboard de monitoreo de proceso ETLs para llevar un mejor control y brindar visibilidad de la ejecución 
                de todas las tareas. \r\n\r\nPlantear e implementar propuesta de ordenamiento por capas en GCP; así como un plan de documentación de activos de información existentes y pases
                producción tanto en GCP como en PowerBI. \r\n\r\nIdentificar oportunidades de negocio sobre proyectos de data y Analytics, en Centria y Clientes. \r\n\r\nConocimientos y 
                Requisitos: \r\n\r\nIngeniero(a) de Sistemas, Informático(a) o Industrial o relacionadas. \r\n\r\nExperiencia de 4 años desempeñando funciones de implementación de proyectos 
                de Data & Analytics (Google y PowerBI). \r\n\r\nExperiencia en el desarrollo pipelines de datos ELTs, ETLs para datos estructurados y no estructurados /experiencia técnica 
                trabajando con big data, técnicas y herramientas modernas de análisis de datos; y con una sólida comprensión de los fundamentos matemáticos y estadísticos. \r\n\r\nGCP/
                Power BI intermedio - avanzado / Python ";

                string calificationOfPostulant = @"Eres un asistente experto en Recursos Humanos y selección de Personal. Teniendo la información del puesto buscado, 
                tenes la misión de calificar a un postulante segun la información que se extrajo de su currículum. El puntaje tiene que se de 0 a 100, en donde 0 es que el candidato no tiene ninguna
                capacidad para ocupar el puesto buscado y 100 indica que es el candidato ideal para ese puesto. Además del puntaje, necesito una breve explicación de por qué se le asignó ese puntaje.
                Necesito la respuesta en formato Json en la que se informe los siguientes campos. ""Puesto Buscado"", ""Nombre del postulante"", ""Correo del postulante"", ""Puntaje"" y ""Explicación"" ";

                string promptPdf = "Determine la información necesaria del siguiente archivo pdf extraida del correo electronico:";
                string completionPdf = $"{promptPdf} \n Informacion del puesto buscado: {jobInformation} \n Informacion del postulante: \n{allContent}";

                var responseCalificationCv = await NonStreamingChat(completionPdf, calificationOfPostulant);

                string responseCalificationCvText = responseCalificationCv.Choices[0].Message.Content;
                this.logger.LogInformation($"respuesta extracción generada por openai: \n{responseCalificationCvText}");

                // Define las expresiones regulares para extraer los datos
                string jobNamePattern = "\"Puesto Buscado\": \"(.*?)\"";
                string candidateNamePattern = "\"Nombre del postulante\": (.*?),";
                string candidateEmailPattern = "\"Correo del postulante\": (.*?),";
                string calificationPattern = "\"Puntaje\": (.*?),";
                string explanationPattern = "\"Explicación\": (.*?)";
                

                // Extrae los datos
                string jobName = Regex.Match(responseCalificationCvText, jobNamePattern).Groups[1].Value;
                string candidateName = Regex.Match(responseCalificationCvText, candidateNamePattern).Groups[1].Value;
                string candidateEmail = Regex.Match(responseCalificationCvText, candidateEmailPattern).Groups[1].Value;
                string calification = Regex.Match(responseCalificationCvText, calificationPattern).Groups[1].Value;
                string explanation = Regex.Match(responseCalificationCvText, explanationPattern).Groups[1].Value;


                // Crea un nuevo objeto JSON con los datos extraídos
                JObject jsonPdf = new JObject
                {
                    ["Puesto Buscado"] = jobName,
                    ["Nombre del candidato"] = candidateName,
                    ["Correo del candidato"] = candidateEmail,
                    ["Puntaje"] = calification,
                    ["Explicacion"] = explanation
                };

                await this.dataAccess.ReceivedCvs.InsertAsync(new ReceivedCv
                {
                    JobName = jobName,
                    CandidateName = candidateName,
                    CandidateEmail = candidateEmail,
                    Calification = calification,
                    Explanation = explanation,
                    ProcessingDate = ProcessingDate,
                    FileUri = uriCV
                });
                await this.dataAccess.SaveChangesAsync();
            
                return true;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Error processing cv from storage: {ex.Message}");

            }
            return false;
        }


        /// <summary>
        /// Ask to Azure OpenAI for a chat completion
        /// </summary>

        public async Task<ChatCompletions> NonStreamingChat(string prompt, string systemMessage)
        {
            
            OpenAIClient openaiClient = new OpenAIClient(new Uri(this.azureOpenAIApiEndpoint), new AzureKeyCredential(this.AzureOpenAIApiKey));
            ChatCompletionsOptions options = new ChatCompletionsOptions()
            {
                DeploymentName = this.azureOpenAIChatCompletionModel,
                MaxTokens = int.Parse(this.maxTokens),
                Temperature = (float)0.7,
                NucleusSamplingFactor = (float)0.95,
                FrequencyPenalty = 0,
                PresencePenalty = 0,

            };
            // Limpiar la lista de mensajes antes de agregar un nuevo systemMessage
            options.Messages.Clear();

            options.Messages.Add(new ChatRequestUserMessage(systemMessage));
            options.Messages.Add(new ChatRequestUserMessage(prompt));

            Azure.Response<ChatCompletions> response = await openaiClient.GetChatCompletionsAsync(
                                   chatCompletionsOptions : options, cancellationToken: default);
            ChatCompletions completions = response.Value;
                        
            return completions;
        }

        /// <summary>
        /// Extract Text From Pdf Async
        /// </summary>

        public async Task<IReadOnlyList<Section>> ExtractTextFromPdfAsync(string folder, string filename, string fileUri)
        {
            DocumentAnalysisClient client = new DocumentAnalysisClient(new Uri(this.azureDocumentInteligenceApiEndpoint), new AzureKeyCredential(this.AzureDocumentInteligenceApiKey));
            var options = new AnalyzeDocumentOptions();
            options.Features.Add(DocumentAnalysisFeature.OcrHighResolution);
            var pdfUri = new Uri(fileUri);
            AnalyzeDocumentOperation operation = await client.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-layout", pdfUri, options , cancellationToken: default);
            var analyzeResults = await operation.WaitForCompletionResponseAsync();
            var content = analyzeResults.Content.ToString();
            // Extraer el texto del documento
            
            List<PageDetail> pageMap = new();
            var jsonElement = JsonDocument.Parse(content).RootElement;
            var jsonElementAnalyzeResult = jsonElement.GetProperty("analyzeResult"); ;
            var methodInfo = typeof(AnalyzeResult).GetMethod("DeserializeAnalyzeResult", BindingFlags.NonPublic | BindingFlags.Static);
            var results = (AnalyzeResult)methodInfo.Invoke(null, new object[] { jsonElementAnalyzeResult });
;
            int chunkId = 0;
            int chunkSize = 0;
            var minChunkSize = 100;
            var tokenOverlap = 0;
            var numTokens = 16000;
            var sections = new List<Section>();

            int pageNumber = 0;
            JToken document = JToken.Parse(jsonElementAnalyzeResult.ToString());

            if (document["tables"] != null)
            {
                foreach (var table in document["tables"])
                {
                    pageNumber = int.Parse(table["cells"][0]["boundingRegions"][0]["pageNumber"].ToString());
                    var tableContent = TableToHtml(table);
                    chunkId++;
                    var section = CreateSection(chunkId, folder, filename, tableContent, pageNumber, fileUri);
                    sections.Add(section);
                }
            }


            if (document["paragraphs"] != null)
            {
                var paragraphContent = string.Empty;
                foreach (var paragraph in document["paragraphs"])
                {
                    pageNumber = paragraph["boundingRegions"] != null ? int.Parse(paragraph["boundingRegions"][0]["pageNumber"].ToString()) : 1;
                    if (!IsParagraphInTable(paragraph, document["tables"]))
                    {
                        chunkSize = GPT3Tokenizer.Encode(paragraphContent + paragraph["content"].ToString()).Count;
                        if (chunkSize < numTokens)
                        {
                            paragraphContent = paragraphContent + "\n" + paragraph["content"].ToString();
                        }
                        else
                        {
                            chunkId++;
                            var section = CreateSection(chunkId, folder, filename, paragraphContent, pageNumber, fileUri);
                            sections.Add(section);

                            // overlap logic                    
                            var overlappedText = paragraphContent.Split(new char[] { ' ' });
                            overlappedText = overlappedText.Skip(overlappedText.Length - (int)Math.Round(tokenOverlap / 0.75)).ToArray();
                            paragraphContent = string.Join(' ', overlappedText);
                        }

                    }

                }
                chunkId++;
                // last seccion
                chunkSize = GPT3Tokenizer.Encode(paragraphContent).Count;
                if (chunkSize > minChunkSize)
                {
                    var section = CreateSection(chunkId, folder, filename, paragraphContent, pageNumber, fileUri);
                    sections.Add(section);
                }
            }

            return sections;
        }

        /// <summary>
        /// Converts a table to html
        /// </summary>
        public string TableToHtml(JToken table)
        {
            string tableHtml = "<table>";
            var rows = new List<List<JToken>>();
            var rowCount = int.Parse(table["rowCount"].ToString());
            for (int i = 0; i < rowCount; i++)
            {
                rows.Add(table["cells"].Where(cell => int.Parse(cell["rowIndex"].ToString()) == i).OrderBy(cell => int.Parse(cell["columnIndex"].ToString())).ToList());
            }
            foreach (var rowCells in rows)
            {
                tableHtml += "<tr>";
                foreach (var cell in rowCells)
                {
                    string tag = "td";
                    if (cell["kind"] != null)
                    {
                        if (cell["kind"].ToString() == "columnHeader" || cell["kind"].ToString() == "rowHeader")
                        {
                            tag = "th";
                        }
                    }
                    string cellSpans = "";
                    if (cell["columnSpan"] != null)
                    {
                        if (int.Parse(cell["columnSpan"].ToString()) > 1)
                        {
                            cellSpans += $" colSpan={cell["columnSpan"]}";
                        }
                    }
                    if (cell["rowSpan"] != null)
                    {
                        if (int.Parse(cell["rowSpan"].ToString()) > 1)
                        {
                            cellSpans += $" rowSpan={cell["rowSpan"]}";
                        }
                    }
                    tableHtml += $"<{tag}{cellSpans}>{HttpUtility.HtmlEncode(cell["content"])}</{tag}>";
                }
                tableHtml += " </tr>";
            }
            tableHtml += "</table>";
            return tableHtml;
        }

        /// <summary>
        /// Returns if a paragraph is inside a table
        /// </summary>
        private bool IsParagraphInTable(JToken paragraph, JToken tables)
        {
            if (tables == null) return false;
            foreach (var table in tables)
            {
                foreach (var cell in table["cells"])
                {
                    if (cell["spans"] != null && cell["spans"].Count() > 0)
                    {
                        var cellOffset = int.Parse(cell["spans"][0]["offset"].ToString());
                        if (paragraph["spans"] != null && paragraph["spans"].Count() > 0)
                        {
                            var paragraphOffset = int.Parse(paragraph["spans"][0]["offset"].ToString());
                            if (cell["spans"] != null && cell["spans"].Count() > 0 && paragraphOffset == cellOffset)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Creates a seccion
        /// </summary>        
        private Section CreateSection(int chunkId, string folder, string filename, string content, int pageNumber, string fileUri)
        {
            var pageName = matchInSetRegex().Replace($"{filename}-{chunkId}", "_").TrimStart('_');
            return new Section(
                Id: pageName,
                Content: content,
                Filename: filename,
                Title: filename,
                Page: pageName,
                PageNumber: pageNumber,
                Folder: folder,
                GroupIds: new string[] { },
                Tags: new string[] { },
                FileUri: fileUri,
                Category: string.Empty);

        }



    }
}
