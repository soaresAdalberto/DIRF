using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

internal class FormatCnpjCpf
{
    public FormatCnpjCpf()
    {

    }

    public string FormatCNPJ(string CNPJ)
    {
        return Convert.ToUInt64(CNPJ).ToString(@"00\.000\.000\/0000\-00");
    }

    public string FormatCPF(string CPF)
    {
        return Convert.ToUInt64(CPF).ToString(@"000\.000\.000\-00");
    }

    public string SemFormatacao(string Codigo)
    {
        return Codigo.Replace(".", string.Empty).Replace("-", string.Empty).Replace("/", string.Empty);
    }
}