using Microsoft.Xrm.Sdk;
using System;

namespace PluginsProjetoFinal
{
    public class PluginAlugClienteInadimp : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            // Verifica se o gatilho é do tipo "create" e se a entidade é do tipo "smt_aluguel"
            if (context.MessageName.ToLower() == "create" && context.PrimaryEntityName == "smt_aluguel")
            {
                // Verifica se o contexto do formulário e dos campos
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity targetAluguel = (Entity)context.InputParameters["Target"];

                    // Verifica se o campo "smt_cliente" está presente no aluguel
                    if (targetAluguel.Attributes.Contains("smt_cliente"))
                    {
                        if (targetAluguel.Attributes.Contains("smt_cliente") && targetAluguel.Attributes["smt_cliente"] is EntityReference)
                        {
                            Guid clienteId = ((EntityReference)targetAluguel.Attributes["smt_cliente"]).Id;

                            // Consulta o registro do cliente para obter o valor do campo "Cliente Inadimplente"
                            Entity cliente = service.Retrieve("smt_cliente", clienteId, new Microsoft.Xrm.Sdk.Query.ColumnSet("smt_clienteinadimplente"));

                            // Verifica se o cliente está marcado como "Inadimplente"
                            // Verifica se o cliente está marcado como "Inadimplente"
                            if (cliente.Attributes.Contains("smt_clienteinadimplente") && cliente.GetAttributeValue<OptionSetValue>("smt_clienteinadimplente").Value == 1)
                            {
                                // Cliente é inadimplente, cancela a criação do aluguel e exibe a mensagem
                                throw new InvalidPluginExecutionException("Impossível fazer aluguéis para clientes inadimplentes.");
                            }

                        }                        
                    }
                }
            }
        }
    }
}
