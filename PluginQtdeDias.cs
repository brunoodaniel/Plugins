using Microsoft.Xrm.Sdk;
using System;

namespace PluginsProjetoFinal
{
    public class PluginQtdeDias : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            // Verifica se o gatilho é do tipo "create" ou "update" e se a entidade é do tipo "smt_aluguel"
            if ((context.MessageName.ToLower() == "create" && context.PrimaryEntityName == "smt_aluguel")){
                // Verifica se o contexto do formulário e dos campos
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity targetAluguel = (Entity)context.InputParameters["Target"];

                    // Verifica a existência dos campos necessários
                    if (targetAluguel.Attributes.Contains("smt_aluguelate") && targetAluguel["smt_aluguelate"] != null)
                    {
                        DateTime aluguelDe = (DateTime)targetAluguel["smt_aluguelde"];
                        DateTime aluguelAte = (DateTime)targetAluguel["smt_aluguelate"];

                        // Calcula a diferença em dias
                        int diferencaDias = (int)(aluguelAte - aluguelDe).TotalDays;

                        // Atualiza o campo smt_qtdedias
                        targetAluguel["smt_qtdedias"] = diferencaDias;
                    }
                }
            }
            else if(context.MessageName.ToLower() == "update" && context.PrimaryEntityName == "smt_aluguel")
            {
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity targetAluguel = (Entity)context.InputParameters["Target"];

                    Entity retorno = service.Retrieve("smt_aluguel", targetAluguel.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("smt_aluguelde", "smt_aluguelate", "smt_qtdedias"));

                    DateTime aluguelDe = (DateTime)retorno["smt_aluguelde"];
                    DateTime aluguelAte = (DateTime)retorno["smt_aluguelate"];

                    // Calcula a diferença em dias
                    int diferencaDias = (int)(aluguelAte - aluguelDe).TotalDays;

                    Entity aluguelAtualizado = new Entity(retorno.LogicalName, retorno.Id);

                    aluguelAtualizado["smt_qtdedias"] = diferencaDias;

                    // Atualiza o campo smt_qtdedias
                    service.Update(aluguelAtualizado);
                }
            }
        }
    }
}
