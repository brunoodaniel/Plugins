using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace PluginsProjetoFinal
{
    public class PluginVeiculoDisponivel : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            if (context.MessageName.ToLower() == "create" && context.PrimaryEntityName == "smt_aluguel")
            {
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity targetAluguel = (Entity)context.InputParameters["Target"];

                    // Verifica se o campo "smt_veiculo" está presente no registro de aluguel
                    if (targetAluguel.Contains("smt_veiculo") && targetAluguel["smt_veiculo"] is EntityReference)
                    {
                        // Obtém o ID do veículo relacionado
                        Guid veiculoId = ((EntityReference)targetAluguel["smt_veiculo"]).Id;

                        // Cria uma consulta para obter o registro de veículo
                        ColumnSet veiculoColumns = new ColumnSet("smt_disponivel");
                        Entity veiculo = service.Retrieve("smt_veiculo", veiculoId, veiculoColumns);

                        // Atualiza o campo "smt_disponivel" para 0 (representando "NAO")
                        veiculo["smt_disponivel"] = false;

                        // Salva as alterações no registro de veículo
                        service.Update(veiculo);
                    }
                }
            }
        }
    }
}
