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
    public class PluginRG : IPlugin
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
                    Entity targetRG = (Entity)context.InputParameters["Target"];
                    //Armazena o nome do CPF
                    string RG = string.Empty;

                    if (targetRG.Contains("smt_rg"))
                    {
                        //Armazena o valor do campo CPF 
                        RG = (String)targetRG["smt_rg"];



                        buscarRG(RG, service);



                    }
                }
            }
        }

        public void buscarRG(String RG, IOrganizationService service)
        {
            string xml = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                          <entity name='smt_cliente'>
                            <attribute name='smt_clienteid' />
                            <attribute name='smt_nome' />
                            <attribute name='smt_cpf' />
                            <attribute name='smt_rg' />
                            <attribute name='smt_telefone' />
                            <attribute name='smt_email' />
                            <attribute name='smt_clienteinadimplente' />
                            <attribute name='smt_cidade' />
                            <attribute name='smt_estado' />
                            <attribute name='smt_datadoultimoaluguel' />
                            <attribute name='smt_valortotaldealugueis' />
                            <order attribute='smt_nome' descending='false' />
                            <filter type='and'>
                              <condition attribute='smt_rg' operator='eq' value='{RG}' />
                            </filter>
                          </entity>
                        </fetch>";



            EntityCollection resultado = service.RetrieveMultiple(new FetchExpression(xml));

            //Caso já exista um CPF cadastrado ele irá retornar um erro que já existe um CPF
            //cadastrado e não irá realizar o cadastro atual.
            if (resultado.Entities.Count > 1)
            {
                throw new InvalidPluginExecutionException("Este RG já possui cadastro!");
            }
        }


    }
}