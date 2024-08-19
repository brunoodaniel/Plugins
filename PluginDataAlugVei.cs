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
    public class PluginDataAlugVei : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.
                GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            //Verifica se o gatilho é do tipo “create “e se a entidade é do tipo “smt_cliente” 
            if (context.MessageName.ToLower() == "create" && context.PrimaryEntityName == "smt_aluguel")
            {
                //Verifica se o contexto do formulário e dos campos 
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is
                Entity)
                {
                    Entity targetAluguel = (Entity)context.InputParameters["Target"];

                    if (targetAluguel.Attributes.Contains("smt_aluguelate"))
                    {
                        EntityReference veiculo = (EntityReference)targetAluguel["smt_veiculo"];
                        DateTime aluguelDe = (DateTime)targetAluguel["smt_aluguelde"];
                        DateTime aluguelAte = (DateTime)targetAluguel["smt_aluguelate"];


                        if (aluguelAte > aluguelDe)
                        {
                            buscarVeiculo(veiculo, aluguelDe, aluguelAte, service);
                        }
                        else
                        {
                            throw new InvalidPluginExecutionException("A data final não pode ser menor que a data inicial!");
                        }
                    }
                }
            }
        }

        public void buscarVeiculo(EntityReference veiculo, DateTime aluguelDe, DateTime aluguelAte, IOrganizationService service)
        {
            string xml = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                          <entity name='smt_aluguel'>
                            <attribute name='smt_aluguelid' />
                            <attribute name='smt_veiculo' />
                            <attribute name='smt_pacotefechado' />
                            <attribute name='smt_valortotal' />
                            <attribute name='smt_cliente' />
                            <attribute name='smt_qtdehoras' />
                            <attribute name='smt_aluguelde' />
                            <attribute name='smt_aluguelate' />
                            <attribute name='smt_qtdedias' />
                            <filter type='and'>
                              <condition attribute='smt_veiculo' operator='eq' uiname='ewwr3e3' uitype='smt_veiculo' value='{veiculo.Id}' />
                              <filter type='or'>
                                <filter type='and'>
                                  <condition attribute='smt_aluguelde' operator='on-or-after' value='{aluguelDe}' />
                                  <condition attribute='smt_aluguelate' operator='on-or-before' value='{aluguelAte}' />
                                </filter>
                                <filter type='and'>
                                  <condition attribute='smt_aluguelde' operator='on-or-before' value='{aluguelDe}' />
                                  <condition attribute='smt_aluguelate' operator='on-or-after' value='{aluguelAte}' />
                                </filter>
                                <filter type='and'>
                                  <condition attribute='smt_aluguelde' operator='on-or-before' value='{aluguelDe}' />
                                  <condition attribute='smt_aluguelate' operator='on-or-before' value='{aluguelAte}' />
                                  <condition attribute='smt_aluguelate' operator='on-or-after' value='{aluguelDe}' />
                                </filter>
                                <filter type='and'>
                                  <condition attribute='smt_aluguelde' operator='on-or-after' value='{aluguelDe}' />
                                  <condition attribute='smt_aluguelde' operator='on-or-before' value='{aluguelAte}' />
                                  <condition attribute='smt_aluguelate' operator='on-or-after' value='{aluguelAte}' />
                                </filter>
                              </filter>
                            </filter>
                          </entity>
                        </fetch>";



            EntityCollection resultado = service.RetrieveMultiple(new FetchExpression(xml));

            //Caso já exista um CPF cadastrado ele irá retornar um erro que já existe um CPF
            //cadastrado e não irá realizar o cadastro atual.
            if (resultado.Entities.Count > 0)
            {
                throw new InvalidPluginExecutionException("Este Veículo ja está alugado nesta data!");
            }
        }
    }
}