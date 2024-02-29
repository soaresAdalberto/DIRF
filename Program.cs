using System;
using System.IO;
using iText.Forms;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Utils;
using iText.Layout.Splitting;
using iText.Pdfa;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;

namespace DIRF
{
    internal class Program
    {
        //Main:
        static void Main(string[] args)
        {
            var arquivoPdfCPF = @"C:\DIRF\ReceitaCPF.pdf";
            var arquivoPdfCNPJ = @"C:\DIRF\ReceitaCNPJ.pdf";
            var pastaDestinoCPF = @"C:\DIRF\CPF fatiados";
            var pastaDestinoCNPJ = @"C:\DIRF\CNPJ fatiados";

            string texto = ExtraiTexto(arquivoPdfCPF);
            EncontrarCPF(texto, arquivoPdfCPF, pastaDestinoCPF);

            texto = ExtraiTexto(arquivoPdfCNPJ);
            EncontrarCNPJ(texto, arquivoPdfCNPJ, pastaDestinoCNPJ);

            ExcluirArquivosComFinal_2(pastaDestinoCNPJ);
            Console.ReadKey();
        }

        //Método para realizar a extração do texto:
        static string ExtraiTexto(string nomeArquivo)
        {
            string result = null;

            PdfReader pdfReader = new PdfReader(nomeArquivo);
            PdfDocument pdfDoc = new PdfDocument(pdfReader);

            for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
            {
                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                string conteudoPag = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page), strategy).ReplaceLineEndings("\t");

                result += conteudoPag;
            }

            pdfDoc.Close();
            pdfReader.Close();

            return result;
        }

        //Método para procurar CPF dentro do PDF:
        static void EncontrarCPF(string texto, string arquivoPdfCPF, string pastaDestinoCPF)
        {
            FormatCnpjCpf f = new FormatCnpjCpf();

            // Regex para localizar CPF
            string cpfPattern = @"\b\d{3}\.\d{3}\.\d{3}-\d{2}\b";

            // Regex para localizar "Pág. n"
            string paginaPattern = @"Pág\.\s*\d+";

            MatchCollection cpfMatches = Regex.Matches(texto, cpfPattern);
            MatchCollection paginaMatches = Regex.Matches(texto, paginaPattern);

            int posCpf = 0;
            int posPagina = 0;

            //Extraindo as páginas:
            foreach (Match cpfMatch in cpfMatches)
            {
                string cpfAtual = f.SemFormatacao(cpfMatches[posCpf].Value);

                // Verifica se há uma página subsequente com o mesmo CPF
                if (posPagina < paginaMatches.Count && paginaMatches[posPagina].Index < cpfMatches[posCpf].Index)
                {
                    // Adiciona sufixo para evitar conflitos de nome de arquivo
                    int sufixo = 2;
                    while (File.Exists(arquivoPdfCPF))
                    {
                        arquivoPdfCPF = $"{pastaDestinoCPF}\\{cpfAtual}_{sufixo}.pdf";
                        sufixo++;
                    }
                }

                try
                {
                    using (var pdf = new PdfReader(arquivoPdfCPF))
                    {
                        using (var doc = new PdfDocument(pdf))
                        {
                            for (int page = 1; page <= doc.GetNumberOfPages(); page++)
                            {
                                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                                string conteudoPag = PdfTextExtractor.GetTextFromPage(doc.GetPage(page), strategy).ReplaceLineEndings("\t");
                            }

                            if (doc.GetNumberOfPages() == 0)
                            {
                                Console.WriteLine("Arquivo " + arquivoPdfCPF + " não possui páginas.");
                            }
                            else
                            {
                                for (int i = 1; i <= doc.GetNumberOfPages(); i++)
                                {
                                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                                    string conteudoPag = PdfTextExtractor.GetTextFromPage(doc.GetPage(i), strategy).ReplaceLineEndings("\t");

                                    string[] test = conteudoPag.Split("\t");

                                    test[14] = Regex.Replace(test[14], "[^0-9]", "");

                                    if (test[14].Length == 11)
                                    {
                                        using (var pdfNovo = new PdfWriter(pastaDestinoCPF + "\\" + test[14] + ".pdf"))
                                        {
                                            using (var docNovo = new PdfDocument(pdfNovo))
                                            {
                                                doc.CopyPagesTo(i, i, docNovo);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (i > 1)
                                        {
                                            ITextExtractionStrategy strategyAnterior = new SimpleTextExtractionStrategy();
                                            string conteudoPagAnterior = PdfTextExtractor.GetTextFromPage(doc.GetPage(i - 1), strategyAnterior).ReplaceLineEndings("\t");
                                            string[] testAnterior = conteudoPagAnterior.Split("\t");
                                            testAnterior[14] = Regex.Replace(testAnterior[14], "[^0-9]", "");

                                            using (var pdfNovo = new PdfWriter(pastaDestinoCPF + "\\" + testAnterior[14] + ".pdf"))
                                            {
                                                using (var docNovo = new PdfDocument(pdfNovo))
                                                {
                                                    // Copia a página atual e a página anterior para o novo documento
                                                    doc.CopyPagesTo(i - 1, i, docNovo);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro: " + ex.Message);
                }
            }

        }

        //Método para procurar CNPJ dentro do PDF:
        static void EncontrarCNPJ(string texto, string arquivoPdfCNPJ, string pastaDestinoCNPJ)
        {
            FormatCnpjCpf f = new FormatCnpjCpf();

            string cnpjPattern = @"\b\d{2}\.\d{3}\.\d{3}/\d{4}-\d{2}\b";

            // Regex para localizar "Pág. n"
            string paginaPattern = @"Pág\.\s*\d+";

            MatchCollection cnpjMatches = Regex.Matches(texto, cnpjPattern);
            MatchCollection paginaMatches = Regex.Matches(texto, paginaPattern);

            int posCnpj = 0;
            int posPagina = 0;

            // Extraindo as páginas:
            foreach (Match cnpjMatch in cnpjMatches)
            {
                // Loop usando for para comparar valores consecutivos
                string cnpjAtual = f.SemFormatacao(cnpjMatches[posCnpj].Value);

                try
                {
                    using (var pdf = new PdfReader(arquivoPdfCNPJ))
                    {
                        using (var doc = new PdfDocument(pdf))
                        {
                            bool encontrouRendimentos = false;
                            bool encontrouRetencao = false;
                            string cnpjRendimentos = "";
                            string cnpjRetencao = "";

                            for (int i = 1; i <= doc.GetNumberOfPages(); i++)
                            {
                                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                                string conteudoPag = PdfTextExtractor.GetTextFromPage(doc.GetPage(i), strategy).ReplaceLineEndings("\t");

                                string[] test = conteudoPag.Split("\t");

                                // validar posições de cnpj...
                                test[11] = Regex.Replace(test[11], "[^0-9]", "");
                                test[10] = Regex.Replace(test[10], "[^0-9]", "");

                                if (test[2] == "COMPROVANTE ANUAL DE RENDIMENTOS PAGOS OU")
                                {
                                    encontrouRendimentos = true;
                                    cnpjRendimentos = test[11];

                                    using (var pdfNovo = new PdfWriter(pastaDestinoCNPJ + "\\" + test[11] + ".pdf"))
                                    {
                                        using (var docNovo = new PdfDocument(pdfNovo))
                                        {
                                            doc.CopyPagesTo(i, i, docNovo);
                                        }
                                    }
                                }
                                else if (test[2] == "COMPROVANTE ANUAL DE RETENÇÃO DE")
                                {
                                    encontrouRetencao = true;
                                    cnpjRetencao = test[10];

                                    using (var pdfNovo = new PdfWriter(pastaDestinoCNPJ + "\\" + test[10] + "_2" + ".pdf"))
                                    {
                                        using (var docNovo = new PdfDocument(pdfNovo))
                                        {
                                            doc.CopyPagesTo(i, i, docNovo);
                                        }
                                    }
                                }

                                // Combinar PDFs se ambos forem encontrados e cnpjRendimentos for igual a cnpjRetencao
                                if (encontrouRendimentos && encontrouRetencao && cnpjRendimentos == cnpjRetencao)
                                {
                                    // Combine os valores de cnpjRendimentos e cnpjRetencao para formar um novo nome de arquivo
                                    string novoNomeArquivo = $"{pastaDestinoCNPJ}\\{cnpjRendimentos}.pdf";

                                    using (var pdfNovo = new PdfWriter(novoNomeArquivo))
                                    {
                                        using (var docNovo = new PdfDocument(pdfNovo))
                                        {
                                            //AdicionePaginasAoDocumento(cnpjRendimentos, docNovo, doc);
                                            //AdicionePaginasAoDocumento(cnpjRetencao, docNovo, doc);
                                            AdicionePaginasAoDocumento(cnpjRendimentos, docNovo, doc);
                                            AdicionePaginasAoDocumento(cnpjRetencao, docNovo, doc);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine("Erro: " + ex.Message);
                }
            }
        }

        // Função para adicionar páginas ao documento
        static void AdicionePaginasAoDocumento(string cnpj, PdfDocument docNovo, PdfDocument doc)
        {
            if (docNovo.GetNumberOfPages() <= 1)
            {
                // Lógica para adicionar páginas ao documento com base no cnpj
                for (int page = 1; page <= doc.GetNumberOfPages(); page++) //<= 2
                {
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    string conteudoPag = PdfTextExtractor.GetTextFromPage(doc.GetPage(page), strategy).ReplaceLineEndings("\t");
                    string[] test = conteudoPag.Split("\t");

                    // Validar posições de cnpj...
                    test[11] = Regex.Replace(test[11], "[^0-9]", "");
                    test[10] = Regex.Replace(test[10], "[^0-9]", "");

                    if (cnpj == test[11] || cnpj == test[10])
                    {
                        doc.CopyPagesTo(page, page, docNovo);
                    }
                }
            }
        }

        // Excluir arquivos com final 2:
        static void ExcluirArquivosComFinal_2(string pasta)
        {
            try
            {
                // Verifica se a pasta existe
                if (Directory.Exists(pasta))
                {
                    // Obtém a lista de arquivos na pasta
                    string[] arquivos = Directory.GetFiles(pasta);

                    // Itera sobre cada arquivo na pasta
                    foreach (string arquivo in arquivos)
                    {
                        // Verifica se o arquivo termina com "_2"
                        if (arquivo.EndsWith("_2.pdf"))
                        {
                            // Exclui o arquivo
                            File.Delete(arquivo);
                            Console.WriteLine($"Arquivo excluído: {arquivo}");
                        }
                    }

                    Console.WriteLine("Exclusão concluída.");
                }
                else
                {
                    Console.WriteLine("A pasta não existe.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }
    }
}