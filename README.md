# DIRF 2023

## Explicações gerais

Para realização deste projeto, foi necessário o desenvolvimento de uma aplicação que, após receber o arquivo emitido pela Receita Federal, realizasse as seguintes tratativas:

- Realizasse a leitura de um PDF com 2883 páginas, extraindo os CPF (para pessoa física) e CNPJ (para pessoa jurídica) e validando no banco de dados da empresa se é um prestador de serviços (qualificações 06, 07, 08, 11, 12, 13, 14, 15 e 21).

- Após a primeira validação, caso não fosse prestador de serviços (iremos denomimar como PS) era necessário remover a página do arquivo. Sendo PS e PJ, salvamos na pasta ArquivosPJ. Caso contrário, PS e PF, na ArquivosPF. 

- Foi possível, através da análise realizada, identificar que os arquivos de PJ sempre são duplos e distintos, sendo um para os rendimentos e outro para tributos. 

- Já os de PF é apenas um, podendo ter até duas páginas, de acordo com a quantidade de informações disponíveis.

- Com as validações realizadas, os arquivos foram salvos, em suas respectivas páginas, com o número do CPF ou CNPJ correspondente ao documento.

- Por fim, os dados foram disponibilizados através do utilitário DIRF, um projeto da Unimed Divinópolis que atende às necessidades legais acerca dos impostos sobre a renda retido na fonte.

## Tecnologias utilizadas

Este documento descreve o código fonte da aplicação que realizou a divisão dos arquivos, no qual foi criado um projeto em .NET 7.0, que tem por intuito realizar a análise da documentação enviada pela Receita Federal e sua respectiva distribuição aos usuários interessados. O código depende dos seguintes assemblies:

iText.Kernel
iText.Pdfa
System.Text.RegularExpressions

## Classes

O código possui as seguintes classes:

1. Program: Esta classe contém o ponto de entrada do programa (Main) e outros métodos auxiliares para processar arquivos PDF.

-Métodos:

Main: O método principal do programa. Ele recebe os argumentos de linha de comando (que não são usados no código atual) e executa as seguintes etapas:

-Extrai o texto do arquivo PDF contendo CPF (receita_cpf.pdf).
-Procura CPFs no texto extraído e salva cada página contendo um CPF em um arquivo separado na pasta "CPF fatiados".
-Extrai o texto do arquivo PDF contendo CNPJ (receita_cnpj.pdf).
-Procura CNPJs no texto extraído e salva cada página contendo um CNPJ em um arquivo separado na pasta "CNPJ fatiados".
-Exclui arquivos que terminam com "_2.pdf" na pasta "CNPJ fatiados" (provavelmente para lidar com arquivos duplicados criados durante o processamento).
-Aguarda a entrada do usuário pressionando qualquer tecla para finalizar o programa.
-ExtraiTexto: Este método recebe o caminho de um arquivo PDF como entrada e retorna o texto extraído de todas as páginas do arquivo.

-EncontrarCPF: Este método recebe o texto extraído de um PDF, o caminho do arquivo PDF original e o caminho da pasta de destino como entrada. Ele percorre o texto procurando por padrões que correspondam ao formato de CPF usando uma expressão regular. Para cada CPF encontrado, ele verifica se há uma página subsequente com o mesmo CPF. Se houver, ele adiciona um sufixo ao nome do arquivo de saída para evitar conflitos. Em seguida, ele itera sobre todas as páginas do PDF e verifica se o conteúdo da página contém o CPF. Se contiver, ele copia a página para um novo arquivo PDF na pasta de destino usando o CPF como nome do arquivo.

-EncontrarCNPJ: Este método funciona de forma semelhante ao método EncontrarCPF, mas procura por CNPJs usando uma expressão regular específica para o formato de CNPJ. Além disso, ele verifica se o texto da página contém os termos "COMPROVANTE ANUAL DE RENDIMENTOS PAGOS OU" e "COMPROVANTE ANUAL DE RETENÇÃO DE" para diferenciar entre arquivos de rendimentos e retenção. Se ambos os CNPJs forem encontrados e corresponderem, ele combina os PDFs de rendimentos e retenção em um único arquivo PDF.

-AdicionePaginasAoDocumento: Este método recebe o CNPJ, um documento PDF de destino e um documento PDF de origem como entrada. Ele itera sobre todas as páginas do documento PDF de origem e verifica se o conteúdo da página contém o CNPJ fornecido. Se contiver, ele copia a página para o documento PDF de destino.

-ExcluirArquivosComFinal_2: Este método recebe o caminho de uma pasta como entrada. Ele verifica se a pasta existe e, em caso afirmativo, itera sobre todos os arquivos na pasta. Se o nome do arquivo terminar com "_2.pdf", ele exclui o arquivo.

____________________________

2. FormataCnpjCpf: Esta classe fornece métodos para formatar, remover formatação e potencialmente validar números de Cadastro Nacional de Pessoas Jurídicas (CNPJ) e Cadastro de Pessoas Físicas (CPF) no Brasil.

-Métodos:

public string FormatCNPJ(string CNPJ): Recebe uma string CNPJ sem formatação e retorna uma string formatada de acordo com o padrão brasileiro (XX.XXX.XXX/XXXX-XX).

-Converte a string de entrada em um inteiro sem sinal de 64 bits (UInt64) usando Convert.ToUInt64. (Observação: Esta conversão pode não ser necessária e pode causar problemas se a string de entrada não for uma sequência numérica válida. Considere usar validação com expressão regular para garantir que a entrada corresponda ao formato esperado antes da conversão.)
-Formata o valor convertido usando uma string de formato composta (@"00\.000\.000\/0000\-00") para obter a saída desejada.

public string FormatCPF(string CPF): Funciona de forma semelhante a FormatCNPJ, mas recebe uma string CPF sem formatação e a retorna formatada de acordo com o padrão brasileiro (XXX.XXX.XXX-XX).

-Realiza a mesma conversão e formatação como em FormatCNPJ. (**Mesma recomendação de validação com expressão regular se aplicável.)

public string SemFormatacao(string Codigo): Remove todos os caracteres de formatação (pontos, hífens e barras) de uma dada string (presumivelmente representando um CNPJ ou CPF).

-Utiliza operações de substituição de string (Replace) para remover os caracteres de formatação especificados.
