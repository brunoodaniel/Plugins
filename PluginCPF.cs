using Microsoft.SqlServer.Server;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows;
using System.Xml.Linq;


namespace PluginsProjetoFinal
{
    public class PluginCPF : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.
                GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            //Verifica se o gatilho é do tipo “create “e se a entidade é do tipo “smt_cliente” 
            if (context.MessageName.ToLower() == "create" && context.PrimaryEntityName == "smt_cliente")
            {
                //Verifica se o contexto do formulário e dos campos 
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is
                Entity)
                {
                    Entity targetCPF = (Entity)context.InputParameters["Target"];
                    //Armazena o nome do CPF
                    string CPF = string.Empty;

                    if (targetCPF.Contains("smt_cpf"))
                    {
                        //Armazena o valor do campo CPF 
                        CPF = (String)targetCPF["smt_cpf"];

                        if (!IsCpf(CPF))
                        {
                            throw new InvalidPluginExecutionException("Este CPF é inválido!");
                        }

                        buscarCpf(CPF, service);
                    }
                }
            }
        }

        public void buscarCpf(String CPF, IOrganizationService service)
        {
            string xml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='smt_cliente'>
                                <attribute name='smt_clienteid' />
                                <attribute name='smt_cpf' />
                                <attribute name='smt_nome' />
                                <attribute name='smt_rg' />
                                <attribute name='smt_telefone' />
                                <attribute name='smt_email' />
                                <attribute name='smt_cidade' />
                                <attribute name='smt_estado' />
                                <attribute name='smt_clienteinadimplente' />
                                <attribute name='smt_valortotaldealugueis' />
                                <order attribute='smt_cpf' descending='false' />
                                <filter type='and'>
                                  <condition attribute='statecode' operator='eq' value='0' />
                                  <condition attribute='smt_cpf' operator='eq' value='" + CPF + @"'/>
                                </filter>
                              </entity>
                            </fetch>";



            EntityCollection resultado = service.RetrieveMultiple(new FetchExpression(xml));

            //Caso já exista um CPF cadastrado ele irá retornar um erro que já existe um CPF
            //cadastrado e não irá realizar o cadastro atual.
            if (resultado.Entities.Count > 1)
            {
                throw new InvalidPluginExecutionException("Este CPF já possui cadastro!");
            }
        }

        public bool IsCpf(string cpf)
        {
            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tempCpf;
            string digito;
            int soma;
            int resto;
            cpf = cpf.Trim();
            cpf = cpf.Replace(".", "").Replace("-", "");
            if (cpf.Length != 11)
                return false;
            tempCpf = cpf.Substring(0, 9);
            soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = resto.ToString();
            tempCpf = tempCpf + digito;
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = digito + resto.ToString();
            return cpf.EndsWith(digito);
        }
    }
}